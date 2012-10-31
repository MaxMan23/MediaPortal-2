#region Copyright (C) 2007-2012 Team MediaPortal

/*
    Copyright (C) 2007-2012 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using MediaPortal.Common.Commands;
using MediaPortal.Common.Localization;
using MediaPortal.UI.Control.InputManager;
using MediaPortal.UI.SkinEngine.Commands;
using MediaPortal.UI.SkinEngine.Controls.Animations;
using MediaPortal.UI.SkinEngine.Controls.Brushes;
using MediaPortal.UI.SkinEngine.Controls.ImageSources;
using MediaPortal.UI.SkinEngine.Controls.Panels;
using MediaPortal.UI.SkinEngine.Controls.Transforms;
using MediaPortal.UI.SkinEngine.Controls.Visuals;
using MediaPortal.UI.SkinEngine.Controls.Visuals.Effects;
using MediaPortal.UI.SkinEngine.Controls.Visuals.Shapes;
using MediaPortal.UI.SkinEngine.Controls.Visuals.Styles;
using MediaPortal.UI.SkinEngine.Controls.Visuals.Templates;
using MediaPortal.UI.SkinEngine.Controls.Visuals.Triggers;
using MediaPortal.UI.SkinEngine.MarkupExtensions;
using MediaPortal.UI.SkinEngine.MpfElements.Converters;
using MediaPortal.UI.SkinEngine.MpfElements.Resources;
using MediaPortal.UI.SkinEngine.ScreenManagement;
using MediaPortal.UI.SkinEngine.Xaml.Interfaces;
using MediaPortal.Utilities;
using SlimDX;
using Brush = MediaPortal.UI.SkinEngine.Controls.Brushes.Brush;
using CommandList = MediaPortal.UI.SkinEngine.Commands.CommandList;
using Image = MediaPortal.UI.SkinEngine.Controls.Visuals.Image;
using Matrix = System.Drawing.Drawing2D.Matrix;
using Rectangle = MediaPortal.UI.SkinEngine.Controls.Visuals.Shapes.Rectangle;
using TypeConverter = MediaPortal.UI.SkinEngine.Xaml.TypeConverter;

namespace MediaPortal.UI.SkinEngine.MpfElements
{
  /// <summary>
  /// This class holds a registration for all elements which can be instantiated  by a XAML file. It also provides
  /// static methods for type conversions between special types and for copying instances.
  /// </summary>
  public class MPF
  {
    protected static readonly IEnumerable<IBinding> EMPTY_BINDING_ENUMERATION = new List<IBinding>();

    #region Variables

    protected static readonly NumberFormatInfo NUMBERFORMATINFO = CultureInfo.InvariantCulture.NumberFormat;

    /// <summary>
    /// Registration for all elements the loader can create from a XAML file.
    /// </summary>
    protected static IDictionary<string, Type> _objectClassRegistrations = new Dictionary<string, Type>();

    static MPF()
    {
      // Screen
      _objectClassRegistrations.Add("Screen", typeof(Screen));

      // Panels
      _objectClassRegistrations.Add("DockPanel", typeof(DockPanel));
      _objectClassRegistrations.Add("StackPanel", typeof(StackPanel));
      _objectClassRegistrations.Add("VirtualizingStackPanel", typeof(VirtualizingStackPanel));
      _objectClassRegistrations.Add("Canvas", typeof(Canvas));
      _objectClassRegistrations.Add("Grid", typeof(Grid));
      _objectClassRegistrations.Add("RowDefinition", typeof(RowDefinition));
      _objectClassRegistrations.Add("ColumnDefinition", typeof(ColumnDefinition));
      _objectClassRegistrations.Add("GridLength", typeof(GridLength));
      _objectClassRegistrations.Add("WrapPanel", typeof(WrapPanel));
      _objectClassRegistrations.Add("UniformGrid", typeof(UniformGrid));

      // Visuals
      _objectClassRegistrations.Add("ARRetainingControl", typeof(ARRetainingControl));
      _objectClassRegistrations.Add("BackgroundEffect", typeof(BackgroundEffect));
      _objectClassRegistrations.Add("Control", typeof(Controls.Visuals.Control));
      _objectClassRegistrations.Add("ContentControl", typeof(ContentControl));
      _objectClassRegistrations.Add("Border", typeof(Border));
      _objectClassRegistrations.Add("GroupBox", typeof(GroupBox));
      _objectClassRegistrations.Add("Image", typeof(Image));
      _objectClassRegistrations.Add("Button", typeof(Button));
      _objectClassRegistrations.Add("RadioButton", typeof(RadioButton));
      _objectClassRegistrations.Add("CheckBox", typeof(CheckBox));
      _objectClassRegistrations.Add("Label", typeof(Label));
      _objectClassRegistrations.Add("ListView", typeof(ListView));
      _objectClassRegistrations.Add("ListViewItem", typeof(ListViewItem));
      _objectClassRegistrations.Add("ContentPresenter", typeof(ContentPresenter));
      _objectClassRegistrations.Add("ScrollContentPresenter", typeof(ScrollContentPresenter));
      _objectClassRegistrations.Add("ProgressBar", typeof(ProgressBar));
      _objectClassRegistrations.Add("HeaderedItemsControl", typeof(HeaderedItemsControl));
      _objectClassRegistrations.Add("TreeView", typeof(TreeView));
      _objectClassRegistrations.Add("TreeViewItem", typeof(TreeViewItem));
      _objectClassRegistrations.Add("ItemsPresenter", typeof(ItemsPresenter));
      _objectClassRegistrations.Add("ScrollViewer", typeof(ScrollViewer));
      _objectClassRegistrations.Add("TextBox", typeof(TextBox));
      _objectClassRegistrations.Add("TextControl", typeof(TextControl));
      _objectClassRegistrations.Add("KeyBinding", typeof(KeyBinding));
      _objectClassRegistrations.Add("KeyBindingControl", typeof(KeyBindingControl));
      _objectClassRegistrations.Add("VirtualKeyboardControl", typeof(VirtualKeyboardControl));
      _objectClassRegistrations.Add("VirtualKeyboardPresenter", typeof(VirtualKeyboardPresenter));
      _objectClassRegistrations.Add("Thickness", typeof(Thickness));

      // Image Sources
      _objectClassRegistrations.Add("BitmapImageSource", typeof(BitmapImageSource));
      _objectClassRegistrations.Add("MultiImageSource", typeof(MultiImageSource));
      _objectClassRegistrations.Add("ImagePlayerImageSource", typeof(ImagePlayerImageSource));

      // Brushes
      _objectClassRegistrations.Add("SolidColorBrush", typeof(SolidColorBrush));
      _objectClassRegistrations.Add("LinearGradientBrush", typeof(LinearGradientBrush));
      _objectClassRegistrations.Add("RadialGradientBrush", typeof(RadialGradientBrush));
      _objectClassRegistrations.Add("ImageBrush", typeof(ImageBrush));
      _objectClassRegistrations.Add("VisualBrush", typeof(VisualBrush));
      _objectClassRegistrations.Add("VideoBrush", typeof(VideoBrush));
      _objectClassRegistrations.Add("GradientBrush", typeof(GradientBrush));
      _objectClassRegistrations.Add("GradientStopCollection", typeof(GradientStopCollection));
      _objectClassRegistrations.Add("GradientStop", typeof(GradientStop));

      // Shapes
      _objectClassRegistrations.Add("Rectangle", typeof(Rectangle));
      _objectClassRegistrations.Add("Ellipse", typeof(Ellipse));
      _objectClassRegistrations.Add("Line", typeof(Line));
      _objectClassRegistrations.Add("Polygon", typeof(Polygon));
      _objectClassRegistrations.Add("Path", typeof(Path));
      _objectClassRegistrations.Add("Shape", typeof(Shape));

      // Animations
      _objectClassRegistrations.Add("ColorAnimation", typeof(ColorAnimation));
      _objectClassRegistrations.Add("DoubleAnimation", typeof(DoubleAnimation));
      _objectClassRegistrations.Add("PointAnimation", typeof(PointAnimation));
      _objectClassRegistrations.Add("Storyboard", typeof(Storyboard));
      _objectClassRegistrations.Add("ColorAnimationUsingKeyFrames", typeof(ColorAnimationUsingKeyFrames));
      _objectClassRegistrations.Add("DoubleAnimationUsingKeyFrames", typeof(DoubleAnimationUsingKeyFrames));
      _objectClassRegistrations.Add("PointAnimationUsingKeyFrames", typeof(PointAnimationUsingKeyFrames));
      _objectClassRegistrations.Add("SplineColorKeyFrame", typeof(SplineColorKeyFrame));
      _objectClassRegistrations.Add("SplineDoubleKeyFrame", typeof(SplineDoubleKeyFrame));
      _objectClassRegistrations.Add("SplinePointKeyFrame", typeof(SplinePointKeyFrame));
      _objectClassRegistrations.Add("ObjectAnimationUsingKeyFrames", typeof(ObjectAnimationUsingKeyFrames));
      _objectClassRegistrations.Add("DiscreteObjectKeyFrame", typeof(DiscreteObjectKeyFrame));

      // Triggers
      _objectClassRegistrations.Add("EventTrigger", typeof(EventTrigger));
      _objectClassRegistrations.Add("Trigger", typeof(Trigger));
      _objectClassRegistrations.Add("DataTrigger", typeof(DataTrigger));
      _objectClassRegistrations.Add("BeginStoryboard", typeof(BeginStoryboard));
      _objectClassRegistrations.Add("StopStoryboard", typeof(StopStoryboard));
      _objectClassRegistrations.Add("TriggerCommand", typeof(TriggerCommand));
      _objectClassRegistrations.Add("SoundPlayerAction", typeof(SoundPlayerAction));

      // Transforms
      _objectClassRegistrations.Add("TransformGroup", typeof(TransformGroup));
      _objectClassRegistrations.Add("ScaleTransform", typeof(ScaleTransform));
      _objectClassRegistrations.Add("SkewTransform", typeof(SkewTransform));
      _objectClassRegistrations.Add("RotateTransform", typeof(RotateTransform));
      _objectClassRegistrations.Add("TranslateTransform", typeof(TranslateTransform));
      _objectClassRegistrations.Add("MatrixTransform", typeof(MatrixTransform));

      // Styles
      _objectClassRegistrations.Add("Style", typeof(Style));
      _objectClassRegistrations.Add("Setter", typeof(Setter));
      _objectClassRegistrations.Add("BindingSetter", typeof(BindingSetter));
      _objectClassRegistrations.Add("ControlTemplate", typeof(ControlTemplate));
      _objectClassRegistrations.Add("ItemsPanelTemplate", typeof(ItemsPanelTemplate));
      _objectClassRegistrations.Add("DataTemplate", typeof(DataTemplate));
      _objectClassRegistrations.Add("DataStringProvider", typeof(DataStringProvider));
      _objectClassRegistrations.Add("SubItemsProvider", typeof(SubItemsProvider));

      // Resources/wrapper classes
      _objectClassRegistrations.Add("ResourceDictionary", typeof(ResourceDictionary));
      _objectClassRegistrations.Add("Include", typeof(Include));
      _objectClassRegistrations.Add("LateBoundValue", typeof(LateBoundValue));
      _objectClassRegistrations.Add("ResourceWrapper", typeof(ResourceWrapper));
      _objectClassRegistrations.Add("BindingWrapper", typeof(BindingWrapper));

      // Command
      _objectClassRegistrations.Add("CommandList", typeof(CommandList));
      _objectClassRegistrations.Add("InvokeCommand", typeof(InvokeCommand));
      _objectClassRegistrations.Add("CommandBridge", typeof(CommandBridge));

      // Converters
      _objectClassRegistrations.Add("ReferenceNotNull_BoolConverter", typeof(ReferenceNotNull_BoolConverter));
      _objectClassRegistrations.Add("EmptyString2FalseConverter", typeof(EmptyString2FalseConverter));
      _objectClassRegistrations.Add("ExpressionMultiValueConverter", typeof(ExpressionMultiValueConverter));
      _objectClassRegistrations.Add("ExpressionValueConverter", typeof(ExpressionValueConverter));
      _objectClassRegistrations.Add("CommaSeparatedValuesConverter", typeof(CommaSeparatedValuesConverter));
      _objectClassRegistrations.Add("DateFormatConverter", typeof(DateFormatConverter));

      // Markup extensions
      _objectClassRegistrations.Add("StaticResource", typeof(StaticResourceMarkupExtension));
      _objectClassRegistrations.Add("DynamicResource", typeof(DynamicResourceMarkupExtension));
      _objectClassRegistrations.Add("ThemeResource", typeof(ThemeResourceMarkupExtension));
      _objectClassRegistrations.Add("Binding", typeof(BindingMarkupExtension));
      _objectClassRegistrations.Add("MultiBinding", typeof(MultiBindingMarkupExtension));
      _objectClassRegistrations.Add("TemplateBinding", typeof(TemplateBindingMarkupExtension));
      _objectClassRegistrations.Add("PickupBinding", typeof(PickupBindingMarkupExtension));
      _objectClassRegistrations.Add("Command", typeof(CommandMarkupExtension));
      _objectClassRegistrations.Add("CommandStencil", typeof(CommandStencilMarkupExtension));
      _objectClassRegistrations.Add("Model", typeof(GetModelMarkupExtension));
      _objectClassRegistrations.Add("Service", typeof(ServiceRegistrationMarkupExtension));
      _objectClassRegistrations.Add("Color", typeof(ColorMarkupExtension));

      // Others
      _objectClassRegistrations.Add("RelativeSource", typeof(RelativeSource));

      // Effects
      // Image effects based on ImageContext
      _objectClassRegistrations.Add("SimpleImageEffect", typeof(SimpleImageEffect));
      _objectClassRegistrations.Add("ZoomBlurEffect", typeof(ZoomBlurEffect));
      _objectClassRegistrations.Add("PixelateEffect", typeof(PixelateEffect));

      // Generic shader effects based on EffectContext
      _objectClassRegistrations.Add("SimpleShaderEffect", typeof(SimpleShaderEffect));
    }

    #endregion

    #region Public properties

    public static IDictionary<string, Type> ObjectClassRegistrations
    {
      get { return _objectClassRegistrations; }
    }

    #endregion

    #region Public methods

    public static bool ConvertType(object value, Type targetType, out object result)
    {
      result = value;
      if (value == null)
        return true;
      if (value is string && targetType == typeof(Type))
      {
        string typeName = (string) value;
        Type type;
        if (!_objectClassRegistrations.TryGetValue(typeName, out type))
          type = Type.GetType(typeName);
        if (type != null)
        {
          result = type;
          return true;
        }
      }
      // Don't convert LateBoundValue (or superclass ValueWrapper) here... instances of
      // LateBoundValue must stay unchanged until some code part explicitly converts them!
      if (value is ResourceWrapper)
      {
        object resource = ((ResourceWrapper) value).Resource;
        if (TypeConverter.Convert(resource, targetType, out result))
        {
          if (ReferenceEquals(resource, result))
          {
            // Resource must be copied because setters and other controls most probably need a copy of the resource.
            // If we don't copy it, Setter is not able to check if we already return a copy because our input value differs
            // from the output value, even if we didn't do a copy here.
            result = MpfCopyManager.DeepCopyCutLVPs(result);
          }
          return true;
        }
      }
      if (value is string && targetType == typeof(FrameworkElement))
      {
        // It doesn't suffice to have an implicit data template declaration which returns a label for a string.
        // If you try to build a ResourceWrapper with a string and assign that ResourceWrapper to a Button's Content property
        // with a StaticResource, for example, the ResourceWrapper will be assigned directly without the data template being
        // applied. To make it sill work, we need this explicit type conversion here.
        result = new Label { Content = (string) value, Color = Color.White };
        return true;
      }
      if (targetType == typeof(Transform))
      {
        string v = value.ToString();
        string[] parts = v.Split(new[] { ',' });
        if (parts.Length == 6)
        {
          float[] f = new float[parts.Length];
          for (int i = 0; i < parts.Length; ++i)
          {
            object obj;
            TypeConverter.Convert(parts[i], typeof(double), out obj);
            f[i] = (float) obj;
          }
          Matrix matrix2d = new Matrix(f[0], f[1], f[2], f[3], f[4],
            f[5]);
          Static2dMatrix matrix = new Static2dMatrix();
          matrix.Set2DMatrix(matrix2d);
          result = matrix;
          return true;
        }
      }
      else if (targetType == typeof(Vector2))
      {
        result = Convert2Vector2(value.ToString());
        return true;
      }
      else if (targetType == typeof(Vector3))
      {
        result = Convert2Vector3(value.ToString());
        return true;
      }
      else if (targetType == typeof(Vector4))
      {
        result = Convert2Vector4(value.ToString());
        return true;
      }
      else if (targetType == typeof(Thickness))
      {
        Thickness t;
        float[] numberList = ParseFloatList(value.ToString());

        if (numberList.Length == 1)
        {
          t = new Thickness(numberList[0]);
        }
        else if (numberList.Length == 2)
        {
          t = new Thickness(numberList[0], numberList[1]);
        }
        else if (numberList.Length == 4)
        {
          t = new Thickness(numberList[0], numberList[1], numberList[2], numberList[3]);
        }
        else
        {
          throw new ArgumentException("Invalid # of parameters");
        }
        result = t;
        return true;
      }
      else if (targetType == typeof(Brush) && value is string || value is Color)
      {
        try
        {
          Color color = value is Color
            ? (Color) value
            : (Color)
              TypeDescriptor.GetConverter(typeof(Color)).ConvertFromString(value.ToString());
          SolidColorBrush b = new SolidColorBrush
            {
              Color = color
            };
          result = b;
          return true;
        }
        catch (Exception)
        {
          return false;
        }
      }
      else if (targetType == typeof(PointCollection))
      {
        PointCollection coll = new PointCollection();
        string text = value.ToString();
        string[] parts = text.Split(new[] { ',', ' ' });
        for (int i = 0; i < parts.Length; i += 2)
        {
          Point p = new Point(Int32.Parse(parts[i]), Int32.Parse(parts[i + 1]));
          coll.Add(p);
        }
        result = coll;
        return true;
      }
      else if (targetType == typeof(GridLength))
      {
        string text = value.ToString();
        if (text == "Auto")
          result = new GridLength(GridUnitType.Auto, 0.0);
        else if (text == "AutoStretch")
          result = new GridLength(GridUnitType.AutoStretch, 1.0);
        else if (text.IndexOf('*') >= 0)
        {
          int pos = text.IndexOf('*');
          text = text.Substring(0, pos);
          if (text.Length > 0)
          {
            object obj;
            TypeConverter.Convert(text, typeof(double), out obj);
            result = new GridLength(GridUnitType.Star, (double) obj);
          }
          else
            result = new GridLength(GridUnitType.Star, 1.0);
        }
        else
        {
          double v = double.Parse(text);
          result = new GridLength(GridUnitType.Pixel, v);
        }
        return true;
      }
      else if (targetType == typeof(string) && value is IResourceString)
      {
        result = ((IResourceString) value).Evaluate();
        return true;
      }
      else if (targetType.IsAssignableFrom(typeof(IExecutableCommand)) && value is ICommand)
      {
        result = new CommandBridge((ICommand) value);
        return true;
      }
      else if (targetType == typeof(Key) && value is string)
      {
        string str = (string) value;
        // Try a special key
        result = Key.GetSpecialKeyByName(str);
        if (result == null)
          if (str.Length != 1)
            throw new ArgumentException(string.Format("Cannot convert '{0}' to type Key", str));
          else
            result = new Key(str[0]);
        return true;
      }
      else if (targetType == typeof(string) && value is IEnumerable)
      {
        result = StringUtils.Join(", ", (IEnumerable) value);
        return true;
      }
      result = value;
      return false;
    }

    public static bool CopyMpfObject(object source, out object target)
    {
      target = null;
      if (source == null)
        return true;
      Type t = source.GetType();
      if (t == typeof(Vector2))
      {
        Vector2 vec = (Vector2) source;
        Vector2 result = new Vector2 { X = vec.X, Y = vec.Y };
        target = result;
        return true;
      }
      if (t == typeof(Vector3))
      {
        Vector3 vec = (Vector3) source;
        Vector3 result = new Vector3 { X = vec.X, Y = vec.Y, Z = vec.Z };
        target = result;
        return true;
      }
      if (t == typeof(Vector4))
      {
        Vector4 vec = (Vector4) source;
        Vector4 result = new Vector4 { X = vec.X, Y = vec.Y, W = vec.W, Z = vec.Z };
        target = result;
        return true;
      }
      if (source is IUnmodifiableResource)
      {
        IUnmodifiableResource resource = (IUnmodifiableResource) source;
        if (resource.Owner != null)
        {
          target = source;
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Method to cleanup resources for callers which don't register themselves as owner.
    /// </summary>
    /// <param name="maybeUIElementOrDisposable">Element to be cleaned up or disposed.</param>
    public static void TryCleanupAndDispose(object maybeUIElementOrDisposable)
    {
      IUnmodifiableResource resource = maybeUIElementOrDisposable as IUnmodifiableResource;
      if (resource != null && resource.Owner != null)
        // Optimize disposal for unmodifiable resources: They are only disposed by their parent ResourceDictionary
        return;
      TryCleanupAndDispose_NoCheckOwner(maybeUIElementOrDisposable);
    }

    protected static void TryCleanupAndDispose_NoCheckOwner(object maybeUIElementOrDisposable)
    {
      if (!(maybeUIElementOrDisposable is ISkinEngineManagedObject))
        // Don't dispose external resources
        return;
      UIElement u = maybeUIElementOrDisposable as UIElement;
      if (u != null)
      {
        u.CleanupAndDispose();
        return;
      }
      IDisposable d = maybeUIElementOrDisposable as IDisposable;
      if (d == null)
        return;
      d.Dispose();
    }

    /// <summary>
    /// Sets the owner of the given resource to the given <paramref name="owner"/>, if the given resource implements the
    /// <see cref="IUnmodifiableResource"/> interface. The owner is only set if no owner is set yet, except if <paramref name="force"/> is
    /// set to <c>true</c>.
    /// </summary>
    /// <remarks>
    /// Containers like <see cref="ResourceDictionary"/> or <see cref="Setter"/> set themselves as owner of their contents.
    /// That has two implications:
    /// <list type="bullet">
    /// <item>The owner of a resource is responsible for the disposal of the resource. A resource with an owner cannot live longer than its owner.</item>
    /// <item>A resource with an owner is not copied. Instead, the <see cref="CopyMpfObject"/> method will return the original reference.</item>
    /// </list>
    /// That works only for resources which implement the <see cref="IUnmodifiableResource"/> interface. Those resources are unmodifiable, i.e.
    /// they are not "personalized" to their owner so it is safe to reuse their reference.
    /// So if a container sets itself as owner of its contents, it can optimize the performance if the contents are very often of type
    /// <see cref="IUnmodifiableResource"/>.
    /// </remarks>
    /// <param name="res">Resource to set the owner.</param>
    /// <param name="owner">Owner to be set.</param>
    /// <param name="force">If set to <c>false</c> and the <paramref name="res">resource</paramref> has already an owner, nothing happens.
    /// Else, the owner of the resource will be set.</param>
    public static void SetOwner(object res, object owner, bool force)
    {
      IUnmodifiableResource resource = res as IUnmodifiableResource;
      if (resource != null && (resource.Owner == null || force))
        resource.Owner = owner;
    }

    /// <summary>
    /// Method to cleanup resources for callers which register themselves as owner.
    /// </summary>
    /// <param name="res">Element to be cleaned up or disposed.</param>
    /// <param name="checkOwner">Owner reference to check for. This method will only clean up the given element if the
    /// specified <paramref name="checkOwner"/> is the owner of the given element or if the element doesn't have an owner.</param>
    public static void CleanupAndDisposeResourceIfOwner(object res, object checkOwner)
    {
      IUnmodifiableResource resource = res as IUnmodifiableResource;
      if (resource == null || resource.Owner == null || ReferenceEquals(resource.Owner, checkOwner))
        TryCleanupAndDispose_NoCheckOwner(res);
    }

    #endregion

    #region Private/protected methods

    /// <summary>
    /// Converts a string to a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="coordsString">The coordinates in "0.2,0.4" format. This method
    /// will fill as many coordinates in the result vector as specified in the
    /// comma separated string. So the string "3.5,7.2" will result in a vector (3.5, 7.2),
    /// the string "5.6" will result in a vector (5.6, 0),
    /// an empty or a <code>null</code> string will result in a vector (0, 0).</param>
    /// <returns>New <see cref="Vector2"/> instance with the specified coordinates,
    /// never <code>null</code>.</returns>
    protected static Vector2 Convert2Vector2(string coordsString)
    {
      if (coordsString == null)
      {
        return new Vector2(0, 0);
      }
      Vector2 vec = new Vector2();
      string[] coords = coordsString.Split(new[] { ',' });
      object obj;
      if (coords.Length > 0)
      {
        TypeConverter.Convert(coords[0], typeof(float), out obj);
        vec.X = (float) obj;
      }
      if (coords.Length > 1)
      {
        TypeConverter.Convert(coords[1], typeof(float), out obj);
        vec.Y = (float) obj;
      }
      return vec;
    }

    /// <summary>
    /// Converts a string to a <see cref="Vector3"/>.
    /// </summary>
    /// <param name="coordsString">The coordinates in "0.2,0.4,0.1" format. This method
    /// will fill as many coordinates in the result vector as specified in the
    /// comma separated string. So the string "3.5,7.2,5.2" will result in a vector (3.5, 7.2, 5.2),
    /// the string "5.6" will result in a vector (5.6, 0, 0),
    /// an empty or a <code>null</code> string will result in a vector (0, 0, 0).</param>
    /// <returns>New <see cref="Vector3"/> instance with the specified coordinates,
    /// never <code>null</code>.</returns>
    protected static Vector3 Convert2Vector3(string coordsString)
    {
      if (coordsString == null)
      {
        return new Vector3(0, 0, 0);
      }
      Vector3 vec = new Vector3();
      string[] coords = coordsString.Split(new[] { ',' });
      object obj;
      if (coords.Length > 0)
      {
        TypeConverter.Convert(coords[0], typeof(float), out obj);
        vec.X = (float) obj;
      }
      if (coords.Length > 1)
      {
        TypeConverter.Convert(coords[1], typeof(float), out obj);
        vec.Y = (float) obj;
      }
      if (coords.Length > 2)
      {
        TypeConverter.Convert(coords[2], typeof(float), out obj);
        vec.Z = (float) obj;
      }
      return vec;
    }


    /// <summary>
    /// Converts a string to a <see cref="Vector4"/>.
    /// </summary>
    /// <param name="coordsString">The coordinates in "0.2,0.4,0.1,0.6" format. This method
    /// will fill as many coordinates in the result vector as specified in the
    /// comma separated string. So the string "3.5,7.2,5.2,2.8" will result in a
    /// vector (3.5, 7.2, 5.2, 2.8),
    /// the string "5.6" will result in a vector (5.6, 0, 0, 0),
    /// an empty or a <code>null</code> string will result in a vector (0, 0, 0, 0).</param>
    /// <returns>New <see cref="Vector4"/> instance with the specified coordinates,
    /// never <code>null</code>.</returns>
    protected static Vector4 Convert2Vector4(string coordsString)
    {
      if (coordsString == null)
      {
        return new Vector4(0, 0, 0, 0);
      }
      Vector4 vec = new Vector4();
      string[] coords = coordsString.Split(new[] { ',' });
      object obj;
      if (coords.Length > 0)
      {
        TypeConverter.Convert(coords[0], typeof(float), out obj);
        vec.X = (float) obj;
      }
      if (coords.Length > 1)
      {
        TypeConverter.Convert(coords[1], typeof(float), out obj);
        vec.Y = (float) obj;
      }
      if (coords.Length > 2)
      {
        TypeConverter.Convert(coords[2], typeof(float), out obj);
        vec.Z = (float) obj;
      }
      if (coords.Length > 3)
      {
        TypeConverter.Convert(coords[3], typeof(float), out obj);
        vec.W = (float) obj;
      }
      return vec;
    }

    /// <summary>
    /// Parses a comma separated list of floats.
    /// </summary>
    /// <param name="numbersString">The string representing the list of numbers.</param>
    /// <returns>Array of floats.</returns>
    /// <exception cref="ArgumentException">If the <paramref name="numbersString"/>
    /// is empty or if </exception>
    protected static float[] ParseFloatList(string numbersString)
    {
      string[] numbers = numbersString.Split(new[] { ',' });
      if (numbers.Length == 0)
        throw new ArgumentException("Empty list");
      float[] result = new float[numbers.Length];
      for (int i = 0; i < numbers.Length; i++)
        result[i] = (float) TypeConverter.Convert(numbers[i], typeof(float));
      return result;
    }

    #endregion
  }
}