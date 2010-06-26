#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using MediaPortal.Core;
using MediaPortal.Core.General;
using MediaPortal.Core.Logging;
using MediaPortal.UI.SkinEngine.ScreenManagement;
using MediaPortal.UI.SkinEngine.Xaml;
using SlimDX;
using MediaPortal.UI.Control.InputManager;
using MediaPortal.UI.SkinEngine.Controls.Visuals.Triggers;
using MediaPortal.UI.SkinEngine.Controls.Animations;
using MediaPortal.UI.SkinEngine.Controls.Transforms;
using MediaPortal.UI.SkinEngine.Commands;
using MediaPortal.UI.SkinEngine.MpfElements.Resources;
using MediaPortal.UI.SkinEngine.Xaml.Interfaces;
using MediaPortal.Utilities.DeepCopy;
using MediaPortal.UI.SkinEngine.SkinManagement;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals
{
  #region Additional enums, delegates and classes

  public enum VisibilityEnum
  {
    Visible = 0,
    Hidden = 1,
    Collapsed = 2,
  }

  /// <summary>
  /// Delegate interface which decides if an element fulfills a special condition.
  /// </summary>
  public interface IFinder
  {
    /// <summary>
    /// Query method which decides if the specified <paramref name="current"/>
    /// element fulfills the condition exposed by this class.
    /// </summary>
    /// <returns><c>true</c> if the specified <paramref name="current"/> element
    /// fulfills the condition exposed by this class, else <c>false</c>.</returns>
    bool Query(UIElement current);
  }

  /// <summary>
  /// Finder implementation which returns an element if multiple child finders accept it.
  /// </summary>
  public class MultiFinder : IFinder
  {
    protected IFinder[] _finders;

    public MultiFinder(IFinder[] finders)
    {
      _finders = finders;
    }

    public bool Query(UIElement current)
    {
      foreach (IFinder finder in _finders)
        if (!finder.Query(current))
          return false;
      return true;
    }
  }

  /// <summary>
  /// Finder implementation which returns an element if it is visible.
  /// </summary>
  public class VisibleElementFinder : IFinder
  {
    private static VisibleElementFinder _instance = null;

    public bool Query(UIElement current)
    {
      return current.IsVisible;
    }

    public static VisibleElementFinder Instance
    {
      get
      {
        if (_instance == null)
          _instance = new VisibleElementFinder();
        return _instance;
      }
    }
  }

  /// <summary>
  /// Finder implementation which looks for elements of the specified type.
  /// </summary>
  public class TypeFinder : IFinder
  {
    protected Type _type;

    public TypeFinder(Type type)
    {
      _type = type;
    }

    public bool Query(UIElement current)
    {
      return _type == current.GetType();
    }
  }

  /// <summary>
  /// Finder implementation which looks for elements of a the given type or
  /// of a type derived from the given type.
  /// </summary>
  public class SubTypeFinder : IFinder
  {
    protected Type _type;

    public SubTypeFinder(Type type)
    {
      _type = type;
    }

    public bool Query(UIElement current)
    {
      return _type.IsAssignableFrom(current.GetType());
    }
  }

  /// <summary>
  /// Finder implementation which looks for elements of a specified name.
  /// </summary>
  public class NameFinder : IFinder
  {
    protected string _name;

    public NameFinder(string name)
    {
      _name = name;
    }

    public bool Query(UIElement current)
    {
      return _name == current.Name;
    }
  }

  /// <summary>
  /// Delegate interface which takes an action on an <see cref="UIElement"/>.
  /// </summary>
  public interface IUIElementAction
  {
    /// <summary>
    /// Executes this action on the specified <paramref name="element"/>.
    /// </summary>
    /// <param name="element">The element to execute this action.</param>
    void Execute(UIElement element);
  }

  /// <summary>
  /// UI element action which sets the specified screen to ui elements.
  /// </summary>
  public class SetScreenAction : IUIElementAction
  {
    protected Screen _screen;

    public SetScreenAction(Screen screen)
    {
      _screen = screen;
    }

    public void Execute(UIElement element)
    {
      element.Screen = _screen;
    }
  }

  public delegate void UIEventDelegate(string eventName);

  #endregion

  public abstract class UIElement : Visual, IContentEnabled
  {
    protected static IList<UIElement> EMPTY_UIELEMENT_LIST = new List<UIElement>();
    protected const string LOADED_EVENT = "UIElement.Loaded";

    public const double DELTA_DOUBLE = 0.01;

    #region Protected fields

    protected AbstractProperty _nameProperty;
    protected AbstractProperty _acutalPositionProperty;
    protected AbstractProperty _marginProperty;
    protected AbstractProperty _triggerProperty;
    protected AbstractProperty _renderTransformProperty;
    protected AbstractProperty _renderTransformOriginProperty;
    protected AbstractProperty _layoutTransformProperty;
    protected AbstractProperty _visibilityProperty;
    protected AbstractProperty _isEnabledProperty;
    protected AbstractProperty _opacityMaskProperty;
    protected AbstractProperty _opacityProperty;
    protected AbstractProperty _freezableProperty;
    protected AbstractProperty _templateNameScopeProperty;
    protected SizeF? _availableSize;
    protected RectangleF? _outerRect;
    protected SizeF _desiredSize;
    protected RectangleF _innerRect;
    protected ResourceDictionary _resources;
    protected IExecutableCommand _loaded;
    protected bool _triggersInitialized;
    protected bool _fireLoaded = true;
    private string _tempName = null;

    #endregion

    #region Ctor

    protected UIElement()
    {
      Init();
      Attach();
    }

    void Init()
    {
      _nameProperty = new SProperty(typeof(string), string.Empty);
      _acutalPositionProperty = new SProperty(typeof(PointF), new PointF(0, 0));
      _marginProperty = new SProperty(typeof(Thickness), new Thickness(0, 0, 0, 0));
      _resources = new ResourceDictionary();
      _triggerProperty = new SProperty(typeof(IList<TriggerBase>), new List<TriggerBase>());
      _renderTransformProperty = new SProperty(typeof(Transform), null);
      _layoutTransformProperty = new SProperty(typeof(Transform), null);
      _renderTransformOriginProperty = new SProperty(typeof(Vector2), new Vector2(0, 0));
      _visibilityProperty = new SProperty(typeof(VisibilityEnum), VisibilityEnum.Visible);
      _isEnabledProperty = new SProperty(typeof(bool), true);
      _freezableProperty = new SProperty(typeof(bool), false);
      _opacityProperty = new SProperty(typeof(double), 1.0);
      _templateNameScopeProperty = new SProperty(typeof(INameScope), null);

      _opacityMaskProperty = new SProperty(typeof(Brushes.Brush), null);
    }

    void Attach()
    {
      _marginProperty.Attach(OnLayoutPropertyChanged);
      _visibilityProperty.Attach(OnVisibilityPropertyChanged);
      _opacityProperty.Attach(OnOpacityPropertyChanged);
      _layoutTransformProperty.Attach(OnLayoutTransformPropertyChanged);
    }

    void Detach()
    {
      _marginProperty.Detach(OnLayoutPropertyChanged);
      _visibilityProperty.Detach(OnVisibilityPropertyChanged);
      _opacityProperty.Detach(OnOpacityPropertyChanged);
      _layoutTransformProperty.Detach(OnLayoutTransformPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      UIElement el = (UIElement) source;
      // We do not copy the focus flag, only one element can have focus
      //HasFocus = el.HasFocus;
      _tempName = el.Name;
      ActualPosition = el.ActualPosition;
      Margin = new Thickness(el.Margin);
      Visibility = el.Visibility;
      IsEnabled = el.IsEnabled;
      // TODO Albert78: Implement Freezing
      Freezable = el.Freezable;
      Opacity = el.Opacity;
      Loaded = copyManager.GetCopy(el.Loaded);
      OpacityMask = copyManager.GetCopy(el.OpacityMask);
      object oldLayoutTransform = LayoutTransform;
      LayoutTransform = copyManager.GetCopy(el.LayoutTransform);
      RenderTransform = copyManager.GetCopy(el.RenderTransform);
      RenderTransformOrigin = copyManager.GetCopy(el.RenderTransformOrigin);
      TemplateNameScope = copyManager.GetCopy(el.TemplateNameScope);
      _resources = copyManager.GetCopy(el._resources);

      // Need to manually call this because we are in a detached state
      OnLayoutTransformPropertyChanged(_layoutTransformProperty, oldLayoutTransform);

      foreach (TriggerBase t in el.Triggers)
        Triggers.Add(copyManager.GetCopy(t));
      Attach();

      copyManager.CopyCompleted += OnCopyCompleted;
    }

    public override void Dispose()
    {
      base.Dispose();
      foreach (UIElement child in GetChildren())
        child.Dispose();
    }

    #endregion

    void OnCopyCompleted(ICopyManager copyManager)
    {
      // When copying, the namescopes of our parent objects might not have been initialized yet. This can be the case
      // when the TemplateNamescope property or a LogicalParent property wasn't copied yet, for example.
      // That's why we cannot simply copy the Name property in the DeepCopy method.
      Name = _tempName;
      _tempName = null;
    }

    void OnOpacityPropertyChanged(AbstractProperty property, object oldValue)
    {
    }

    void OnVisibilityPropertyChanged(AbstractProperty property, object oldValue)
    {
      InvalidateParentLayout();
    }

    /// <summary>
    /// Called when a property value has been changed which makes the current layout invalid.
    /// This method will call InvalidateLayout() to invalidate the layout.
    /// </summary>
    /// <param name="property">The property which was changed.</param>
    /// <param name="oldValue">The old value of the property.</param>
    void OnLayoutPropertyChanged(AbstractProperty property, object oldValue)
    {
      InvalidateLayout();
    }

    void OnLayoutTransformChanged(IObservable observable)
    {
      InvalidateLayout();
    }

    void OnLayoutTransformPropertyChanged(AbstractProperty property, object oldValue)
    {
      if (oldValue is Transform)
        ((Transform) oldValue).ObjectChanged -= OnLayoutTransformChanged;
      if (LayoutTransform != null)
        LayoutTransform.ObjectChanged += OnLayoutTransformChanged;
    }

    public void SetResources(ResourceDictionary resources)
    {
      _resources = resources;
    }

    #region Public properties

    /// <summary>
    /// Event handler called for all events defined by their event string like <see cref="LOADED_EVENT"/>.
    /// </summary>
    public event UIEventDelegate EventOccured;

    public IExecutableCommand Loaded
    {
      get { return _loaded; }
      set { _loaded = value; }
    }

    public ResourceDictionary Resources
    {
      get { return _resources; }
    }

    public AbstractProperty OpacityProperty
    {
      get { return _opacityProperty; }
    }

    public double Opacity
    {
      get { return (double) _opacityProperty.GetValue(); }
      set { _opacityProperty.SetValue(value); }
    }

    public AbstractProperty FreezableProperty
    {
      get { return _freezableProperty; }
    }

    public bool Freezable
    {
      get { return (bool)_freezableProperty.GetValue(); }
      set { _freezableProperty.SetValue(value); }
    }

    public AbstractProperty OpacityMaskProperty
    {
      get { return _opacityMaskProperty; }
    }

    public Brushes.Brush OpacityMask
    {
      get { return (Brushes.Brush) _opacityMaskProperty.GetValue(); }
      set { _opacityMaskProperty.SetValue(value); }
    }

    public AbstractProperty IsEnabledProperty
    {
      get { return _isEnabledProperty; }
    }

    public bool IsEnabled
    {
      get { return (bool) _isEnabledProperty.GetValue(); }
      set { _isEnabledProperty.SetValue(value); }
    }

    public AbstractProperty VisibilityProperty
    {
      get { return _visibilityProperty; }
    }

    public VisibilityEnum Visibility
    {
      get { return (VisibilityEnum) _visibilityProperty.GetValue(); }
      set { _visibilityProperty.SetValue(value); }
    }

    public bool IsVisible
    {
      get { return Visibility == VisibilityEnum.Visible; }
      set { Visibility = value ? VisibilityEnum.Visible : VisibilityEnum.Hidden; }
    }

    public AbstractProperty TriggersProperty
    {
      get { return _triggerProperty; }
    }

    public IList<TriggerBase> Triggers
    {
      get { return (IList<TriggerBase>) _triggerProperty.GetValue(); }
    }

    public AbstractProperty ActualPositionProperty
    {
      get { return _acutalPositionProperty; }
    }

    public PointF ActualPosition
    {
      get { return (PointF) _acutalPositionProperty.GetValue(); }
      set { _acutalPositionProperty.SetValue(value); }
    }

    public AbstractProperty NameProperty
    {
      get { return _nameProperty; }
    }

    public string Name
    {
      get { return _nameProperty.GetValue() as string; }
      set
      {
        INameScope ns = FindNameScope();
        if (ns != null)
          ns.UnregisterName(Name);
        _nameProperty.SetValue(value);
        if (ns != null)
          try
          {
            ns.RegisterName(Name, this);
          }
          catch (ArgumentException)
          {
            ServiceScope.Get<ILogger>().Warn("Name '"+Name+"' was registered twice in namescope '"+ns+"'");
          }
      }
    }

    public AbstractProperty MarginProperty
    {
      get { return _marginProperty; }
    }

    public Thickness Margin
    {
      get { return (Thickness)_marginProperty.GetValue(); }
      set { _marginProperty.SetValue(value); }
    }

    public AbstractProperty LayoutTransformProperty
    {
      get { return _layoutTransformProperty; }
    }

    public Transform LayoutTransform
    {
      get { return _layoutTransformProperty.GetValue() as Transform; }
      set { _layoutTransformProperty.SetValue(value); }
    }

    public AbstractProperty RenderTransformProperty
    {
      get { return _renderTransformProperty; }
    }

    public Transform RenderTransform
    {
      get { return (Transform) _renderTransformProperty.GetValue(); }
      set { _renderTransformProperty.SetValue(value); }
    }

    public AbstractProperty RenderTransformOriginProperty
    {
      get { return _renderTransformOriginProperty; }
    }

    public Vector2 RenderTransformOrigin
    {
      get { return (Vector2) _renderTransformOriginProperty.GetValue(); }
      set { _renderTransformOriginProperty.SetValue(value); }
    }

    /// <summary>
    /// Returns the desired size this element calculated based on the available size.
    /// This value denotes the desired size of this element including its <see cref="Margin"/> in the parent's coordinate
    /// system, i.e. with the <see cref="RenderTransform"/> and <see cref="LayoutTransform"/> applied.
    /// </summary>
    public SizeF DesiredSize
    {
      get { return _desiredSize; }
    }

    public bool IsTemplateControlRoot
    {
      get { return TemplateNameScope != null; }
    }

    public AbstractProperty TemplateNameScopeProperty
    {
      get { return _templateNameScopeProperty; }
    }

    public INameScope TemplateNameScope
    {
      get { return (INameScope) _templateNameScopeProperty.GetValue(); }
      set { _templateNameScopeProperty.SetValue(value); }
    }

    #endregion

    #region Layouting

    /// <summary>
    /// Adds the given <paramref name="margin"/> to the specified <param name="size"/> parameter.
    /// </summary>
    /// <remarks>
    /// <see cref="float.NaN"/> values will be preserved, i.e. if a <paramref name="size"/> coordinate
    /// is <see cref="float.NaN"/>, it won't be changed.
    /// </remarks>
    /// <param name="size">Size parameter where the margin will be added.</param>
    /// <param name="margin">Margin to be added.</param>
    public static void AddMargin(ref SizeF size, Thickness margin)
    {
      if (!float.IsNaN(size.Width))
        size.Width += margin.Left + margin.Right;
      if (!float.IsNaN(size.Height))
        size.Height += margin.Top + margin.Bottom;
    }

    /// <summary>
    /// Adds the given <paramref name="margin"/> to the specified <paramref name="rect"/>.
    /// </summary>
    /// <param name="rect">Inner element's rectangle where the margin will be added.</param>
    /// <param name="margin">Margin to be added.</param>
    public static void AddMargin(ref RectangleF rect, Thickness margin)
    {
      rect.X -= margin.Left;
      rect.Y -= margin.Top;

      rect.Width += margin.Left + margin.Right;
      rect.Height += margin.Top + margin.Bottom;
    }

    /// <summary>
    /// Removes the given <paramref name="margin"/> from the specified <param name="size"/> parameter.
    /// </summary>
    /// <remarks>
    /// <see cref="float.NaN"/> values will be preserved, i.e. if a <paramref name="size"/> coordinate
    /// is <see cref="float.NaN"/>, it won't be changed.
    /// </remarks>
    /// <param name="size">Size parameter where the margin will be removed.</param>
    /// <param name="margin">Margin to be removed.</param>
    public static void RemoveMargin(ref SizeF size, Thickness margin)
    {
      if (!float.IsNaN(size.Width))
        size.Width -= margin.Left + margin.Right;
      if (!float.IsNaN(size.Height))
        size.Height -= margin.Top + margin.Bottom;
    }

    /// <summary>
    /// Removes the given <paramref name="margin"/> from the specified <paramref name="rect"/>.
    /// </summary>
    /// <param name="rect">Outer element's rectangle where the margin will be removed.</param>
    /// <param name="margin">Margin to be removed.</param>
    public static void RemoveMargin(ref RectangleF rect, Thickness margin)
    {
      rect.X += margin.Left;
      rect.Y += margin.Top;

      rect.Width -= margin.Left + margin.Right;
      rect.Height -= margin.Top + margin.Bottom;
    }

    /// <summary>
    /// Will make this element scroll the specified <paramref name="element"/> in a visible
    /// position inside this element's borders. The call should also be delegated to the parent element
    /// with the original element and its updated bounds as parameter; this will make all parents also
    /// scroll their visible range.
    /// </summary>
    /// <remarks>
    /// This method will be overridden by classes which can scroll their content. Such a class
    /// will take two actions here:
    /// <list type="bullet">
    /// <item>Scroll the specified <paramref name="element"/> to a visible region inside its borders,
    /// while undoing layout transformations which will be applied to children.</item>
    /// <item>Call this inherited method, which delegates the call to the visual parent.</item>
    /// </list>
    /// The call to the visual parent should use the same <paramref name="element"/> but an updated
    /// <paramref name="elementBounds"/> rectangle.
    /// </remarks>
    /// <param name="element">The original element which should be made visible.</param>
    /// <param name="elementBounds">The element's bounds after the scrolling update has taken place in the
    /// next layout cycle.</param>
    public virtual void MakeVisible(UIElement element, RectangleF elementBounds)
    {
      UIElement parent = VisualParent as UIElement;
      if (parent != null)
        parent.MakeVisible(element, elementBounds);
    }

    /// <summary>
    /// Returns the information if the specified (absolute) coordinates lay in this element's visible range.
    /// </summary>
    /// <param name="x">Absolute X-coordinate.</param>
    /// <param name="y">Absolute Y-coordinate.</param>
    /// <returns><c>true</c> if the specified coordinates lay in this element's visible range.</returns>
    public virtual bool IsInVisibleArea(float x, float y)
    {
      return false;
    }

    /// <summary>
    /// Returns the information if the specified (absolute) coordinates lay in this element's range.
    /// </summary>
    /// <param name="x">Absolute X-coordinate.</param>
    /// <param name="y">Absolute Y-coordinate.</param>
    /// <returns><c>true</c> if the specified coordinates lay in this element's range.</returns>
    public virtual bool IsInArea(float x, float y)
    {
      return false;
    }

    /// <summary>
    /// Returns the information if the specified (absolute) coordinates lay in the specified child's visible range.
    /// </summary>
    /// <param name="child">The child to check.</param>
    /// <param name="x">Absolute X-coordinate.</param>
    /// <param name="y">Absolute Y-coordinate.</param>
    /// <returns><c>true</c> if the specified coordinates lay in the specified child's visible range.</returns>
    public virtual bool IsChildVisibleAt(UIElement child, float x, float y)
    {
      return child.IsInArea(x, y) && IsInVisibleArea(x, y);
    }

    /// <summary>
    /// Measures this element's size and fills the <see cref="DesiredSize"/> property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is the first part of the two-phase measuring process. In this first phase, parent
    /// controls collect all the size requirements of their child controls.
    /// </para>
    /// <para>
    /// An input size value of <see cref="float.NaN"/> in any coordinate denotes that this child control doesn't have a size
    /// constraint in that direction. Coordinates different from <see cref="float.NaN"/> should be considered by this child
    /// control as the maximum available size in that direction. If this element still produces a bigger
    /// <see cref="DesiredSize"/>, the <see cref="Arrange(RectangleF)"/> method might give it a smaller final region.
    /// </para>
    /// </remarks>
    /// <param name="totalSize">Total size of the element including Margins. As input, this parameter
    /// contains the size available for this child control (size constraint). As output, it must be set
    /// to the <see cref="DesiredSize"/> plus <see cref="Margin"/>.</param>
    public virtual void Measure(ref SizeF totalSize)
    {
    }

    /// <summary>
    /// Arranges the UI element and positions it in the finalrect.
    /// </summary>
    /// <param name="outerRect">The final position and size the parent computed for this child element.</param>
    public virtual void Arrange(RectangleF outerRect)
    {
    }

    /// <summary>
    /// Invalidates the layout of this UIElement.
    /// If dimensions change, it will invalidate the parent visual so the parent
    /// will re-layout itself and its children.
    /// </summary>
    public virtual void InvalidateLayout()
    {
      if (Screen != null)
        Screen.InvalidateLayout(this);
    }

    /// <summary>
    /// Invalidates the layout of our visual parent.
    /// The parent will re-layout itself and its children.
    /// </summary>
    public void InvalidateParentLayout()
    {
      UIElement parent = VisualParent as UIElement;
      if (parent != null)
        parent.InvalidateLayout();
    }

    #endregion

    /// <summary>
    /// Finds the resource with the given resource key.
    /// </summary>
    /// <param name="resourceKey">The resource key.</param>
    /// <returns>Resource with the specified key, or <c>null</c> if not found.</returns>
    public object FindResource(object resourceKey)
    {
      if (Resources.ContainsKey(resourceKey))
        return Resources[resourceKey];
      if (LogicalParent is UIElement)
        return ((UIElement) LogicalParent).FindResource(resourceKey);
      return SkinContext.SkinResources.FindStyleResource(resourceKey);
    }

    public void InitializeTriggers()
    {
      if (!_triggersInitialized)
      {
        _triggersInitialized = true;
        foreach (TriggerBase trigger in Triggers)
          trigger.Setup(this);
      }
    }

    public void StartStoryboard(Storyboard board, HandoffBehavior handoffBehavior)
    {
      if (Screen == null)
        return;
      Screen.Animator.StartStoryboard(board, this, handoffBehavior);
    }

    public void StopStoryboard(Storyboard board)
    {
      if (Screen == null)
        return;
      Screen.Animator.StopStoryboard(board, this);
    }

    public void SetValueInRenderThread(IDataDescriptor dataDescriptor, object value)
    {
      if (Screen == null || SkinContext.RenderThread == Thread.CurrentThread)
        dataDescriptor.Value = value;
      else
        Screen.Animator.SetValue(dataDescriptor, value);
    }

    public bool TryGetPendingValue(IDataDescriptor dataDescriptor, out object value)
    {
      if (Screen != null)
        return Screen.Animator.TryGetPendingValue(dataDescriptor, out value);
      value = null;
      return false;
    }

    public virtual void FireEvent(string eventName)
    {
      foreach (TriggerBase trigger in Triggers)
      {
        EventTrigger eventTrig = trigger as EventTrigger;
        if (eventTrig != null)
          if (eventTrig.RoutedEvent == eventName)
            foreach (TriggerAction ta in eventTrig.Actions)
              ta.Execute(this);
      }
      if (eventName == LOADED_EVENT)
      {
        if (_loaded != null)
          _loaded.Execute();
      }
      foreach (UIElement child in GetChildren())
        child.FireEvent(eventName);
      if (EventOccured != null)
        EventOccured(eventName);
    }

    public virtual void OnMouseMove(float x, float y)
    {
      foreach (UIElement child in GetChildren())
      {
        if (!child.IsVisible) continue;
        child.OnMouseMove(x, y);
      }
    }

    /// <summary>
    /// Will be called when a key is pressed before the registered shortcuts are checked.
    /// Derived classes may override this method to implement special priority key handling code.
    /// </summary>
    /// <param name="key">The key. Should be set to 'Key.None' if handled by child.</param> 
    public virtual void OnKeyPreview(ref Key key)
    {
      foreach (UIElement child in GetChildren())
      {
        if (!child.IsVisible) continue;
        child.OnKeyPreview(ref key);
        if (key == Key.None) return;
      }
    }

    /// <summary>
    /// Will be called when a key is pressed. Derived classes may override this method
    /// to implement special key handling code.
    /// </summary>
    /// <param name="key">The key. Should be set to 'Key.None' if handled by child.</param> 
    public virtual void OnKeyPressed(ref Key key)
    {
      foreach (UIElement child in GetChildren())
      {
        if (!child.IsVisible) continue;
        child.OnKeyPressed(ref key);
        if (key == Key.None) return;
      }
    }

    public override INameScope FindNameScope()
    {
      if (this is INameScope)
        return this as INameScope;
      if (TemplateNameScope != null)
        return TemplateNameScope;
      return LogicalParent == null ? Screen : LogicalParent.FindNameScope();
    }

    /// <summary>
    /// Adds all children in the visual tree to the specified <paramref name="childrenOut"/> collection.
    /// </summary>
    /// <param name="childrenOut">Collection to add children to.</param>
    public virtual void AddChildren(ICollection<UIElement> childrenOut) { }

    /// <summary>
    /// Convenience method for <see cref="AddChildren"/>.
    /// </summary>
    /// <returns>Collection of child elements.</returns>
    public ICollection<UIElement> GetChildren()
    {
      ICollection<UIElement> result = new List<UIElement>();
      AddChildren(result);
      return result;
    }

    /// <summary>
    /// Steps the structure down (in direction to the child elements) along
    /// the visual tree to find an <see cref="UIElement"/> which fulfills the
    /// condition specified by <paramref name="finder"/>. This method has to be
    /// overridden in all descendants exposing visual children, for every child
    /// this method has to be called.
    /// This method does a depth-first search.
    /// </summary>
    /// <param name="finder">Callback interface which decides whether an element
    /// is the element searched for.</param>
    /// <returns><see cref="UIElement"/> for which the specified
    /// <paramref name="finder"/> delegate returned <c>true</c>.</returns>
    public UIElement FindElement(IFinder finder)
    {
      return FindElement_BreadthFirst(finder);
    }

    public UIElement FindElement_DepthFirst(IFinder finder)
    {
      Stack<UIElement> searchStack = new Stack<UIElement>();
      IList<UIElement> elementList = new List<UIElement>();
      searchStack.Push(this);
      while (searchStack.Count > 0)
      {
        UIElement current = searchStack.Pop();
        if (finder.Query(current))
          return current;
        elementList.Clear();
        current.AddChildren(elementList);
        foreach (UIElement child in elementList)
          searchStack.Push(child);
      }
      return null;
    }

    public UIElement FindElement_BreadthFirst(IFinder finder)
    {
      LinkedList<UIElement> searchList = new LinkedList<UIElement>(new UIElement[] {this});
      LinkedListNode<UIElement> current;
      while ((current = searchList.First) != null)
      {
        if (finder.Query(current.Value))
          return current.Value;
        searchList.RemoveFirst();
        current.Value.AddChildren(searchList);
      }
      return null;
    }

    public UIElement FindElementInNamescope(string name)
    {
      INameScope nameScope = FindNameScope();
      if (nameScope != null)
        return nameScope.FindName(name) as UIElement;
      return null;
    }

    public void ForEachElementInTree_BreadthFirst(IUIElementAction action)
    {
      LinkedList<UIElement> searchList = new LinkedList<UIElement>(new UIElement[] { this });
      LinkedListNode<UIElement> current;
      while ((current = searchList.First) != null)
      {
        action.Execute(current.Value);
        searchList.RemoveFirst();
        current.Value.AddChildren(searchList);
      }
    }

    public void ForEachElementInTree_DepthFirst(IUIElementAction action)
    {
      Stack<UIElement> searchStack = new Stack<UIElement>();
      IList<UIElement> elementList = new List<UIElement>();
      searchStack.Push(this);
      while (searchStack.Count > 0)
      {
        UIElement current = searchStack.Pop();
        action.Equals(current);
        elementList.Clear();
        current.AddChildren(elementList);
        foreach (UIElement child in elementList)
          searchStack.Push(child);
      }
    }

    public virtual void Initialize()
    {
      if (_fireLoaded)
      {
        FireEvent(LOADED_EVENT);
        _fireLoaded = false;
      }
    }

    public virtual void Allocate()
    {
      foreach (FrameworkElement child in GetChildren())
        child.Allocate();
    }

    public virtual void Deallocate()
    {
      foreach (FrameworkElement child in GetChildren())
        child.Deallocate();
    }

    public override void SetBindingValue(IDataDescriptor dd, object value)
    {
      SetValueInRenderThread(dd, value);
    }

    public void SetScreen(Screen screen)
    {
      ForEachElementInTree_BreadthFirst(new SetScreenAction(screen));
    }

    public static bool InVisualPath(UIElement check, UIElement child)
    {
      Visual current = child;
      while (current != null)
        if (ReferenceEquals(check, current))
          return true;
        else
          current = current.VisualParent;
      return false;
    }

    public static bool IsNear(double x, double y)
    {
      return Math.Abs(x - y) < DELTA_DOUBLE;
    }

    public static bool GreaterThanOrClose(double x, double y)
    {
      return x > y || IsNear(x, y);
    }

    public static bool LessThanOrClose(double x, double y)
    {
      return x< y || IsNear(x, y);
    }

    #region IContentEnabled members

    public bool FindContentProperty(out IDataDescriptor dd)
    {
      return ReflectionHelper.FindMemberDescriptor(this, "Content", out dd);
    }

    #endregion

    #region Base overrides

    public override string ToString()
    {
      string name = Name;
      return GetType().Name + (string.IsNullOrEmpty(name) ? string.Empty : (", Name: '" + name + "'"));
    }

    #endregion
  }
}