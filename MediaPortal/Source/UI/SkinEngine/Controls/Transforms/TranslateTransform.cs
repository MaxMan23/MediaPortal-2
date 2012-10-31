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

using MediaPortal.Common.General;
using SlimDX;
using MediaPortal.Utilities.DeepCopy;

namespace MediaPortal.UI.SkinEngine.Controls.Transforms
{
  public class TranslateTransform : Transform
  {
    #region Protected fields

    protected AbstractProperty _xProperty;
    protected AbstractProperty _yProperty;

    #endregion

    #region Ctor

    public TranslateTransform()
    {
      Init();
      Attach();
    }

    public override void Dispose()
    {
      base.Dispose();
      Detach();
    }

    void Init()
    {
      _xProperty = new SProperty(typeof(double), 0.0);
      _yProperty = new SProperty(typeof(double), 0.0);
    }

    void Attach()
    {
      _yProperty.Attach(OnPropertyChanged);
      _xProperty.Attach(OnPropertyChanged);
    }

    void Detach()
    {
      _yProperty.Detach(OnPropertyChanged);
      _xProperty.Detach(OnPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      TranslateTransform t = (TranslateTransform) source;
      X = t.X;
      Y = t.Y;
      Attach();
    }

    #endregion

    protected void OnPropertyChanged(AbstractProperty property)
    {
      _needUpdate = true;
      Fire();
    }

    public AbstractProperty XProperty
    {
      get { return _xProperty; }
    }

    public double X
    {
      get { return (double) _xProperty.GetValue(); }
      set { _xProperty.SetValue(value); }
    }

    public AbstractProperty YProperty
    {
      get { return _yProperty; }
    }

    public double Y
    {
      get { return (double) _yProperty.GetValue(); }
      set { _yProperty.SetValue(value); }
    }

    public override void UpdateTransform()
    {
      base.UpdateTransform();
      _matrix = Matrix.Translation((float) X, (float) Y, 0);
    }
  }
}
