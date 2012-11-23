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

using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MediaPortal.Common;
using MediaPortal.Common.Logging;

namespace MediaPortal.UI.Control.InputManager
{
  /// <summary>
  /// Represents a standardized input command in MediaPortal 2. There are two kinds of keys: Printable and non-printable
  /// (special) keys. Special keys have their own key constant defined in this class, for example <see cref="Play"/>.
  /// Printable keys like letters, digits and special characters, aren't defined as own instances in this class,
  /// they will be built on demand.
  /// </summary>
  /// <remarks>
  /// Each input device should provide its own mapping of its possible input to a <see cref="Key"/>.
  /// </remarks>
  public class Key: IXmlSerializable
  {
    #region Special key constants

    public static readonly IDictionary<string, Key> NAME2SPECIALKEY = new Dictionary<string, Key>();

    public static readonly Key None = new Key("None");

    // 0-9, *, # are printable
    public static readonly Key Clear = new Key("Clear");
    public static readonly Key Ok = new Key("Ok");
    public static readonly Key Back = new Key("Back");
    public static readonly Key Info = new Key("Info");
    public static readonly Key TeleText = new Key("TeleText");
    public static readonly Key Power = new Key("Power");
    public static readonly Key Fullscreen = new Key("Fullscreen");

    public static readonly Key ZoomMode = new Key("ZoomMode");
    public static readonly Key Play = new Key("Play");
    public static readonly Key Pause = new Key("Pause");
    public static readonly Key PlayPause = new Key("PlayPause"); // Necessary for keyboard mapping of PlayPause key
    public static readonly Key Stop = new Key("Stop");
    public static readonly Key Rew = new Key("Rew");
    public static readonly Key Fwd = new Key("Fwd");
    public static readonly Key Previous = new Key("Previous");
    public static readonly Key Next = new Key("Next");
    public static readonly Key Record = new Key("Record");

    public static readonly Key Mute = new Key("Mute");
    public static readonly Key VolumeUp = new Key("VolumeUp");
    public static readonly Key VolumeDown = new Key("VolumeDown");

    public static readonly Key ChannelUp = new Key("ChannelUp");
    public static readonly Key ChannelDown = new Key("ChannelDown");

    public static readonly Key Start = new Key("Start");
    public static readonly Key RecordedTV = new Key("RecordedTV");
    public static readonly Key Guide = new Key("Guide");
    public static readonly Key LiveTV = new Key("LiveTV");
    public static readonly Key DVDMenu = new Key("DVDMenu");

    public static readonly Key Red = new Key("Red");
    public static readonly Key Green = new Key("Green");
    public static readonly Key Yellow = new Key("Yellow");
    public static readonly Key Blue = new Key("Blue");

    public static readonly Key Up = new Key("Up");
    public static readonly Key Down = new Key("Down");
    public static readonly Key Left = new Key("Left");
    public static readonly Key Right = new Key("Right");
    public static readonly Key PageUp = new Key("PageUp");
    public static readonly Key PageDown = new Key("PageDown");
    public static readonly Key Home = new Key("Home");
    public static readonly Key End = new Key("End");

    public static readonly Key Delete = Clear;
    public static readonly Key Insert = new Key("Insert");
    public static readonly Key BackSpace = Back;
    public static readonly Key Enter = new Key("Enter"); // Different button than Ok on MCE remote
    public static readonly Key Escape = new Key("Escape");
    
    public static readonly Key Close = new Key("Close"); // Close MP2 using key command

    public static readonly Key F1 = new Key("F1");
    public static readonly Key F2 = new Key("F2");
    public static readonly Key F3 = new Key("F3");
    public static readonly Key F4 = new Key("F4");
    public static readonly Key F5 = new Key("F5");
    public static readonly Key F6 = new Key("F6");
    public static readonly Key F7 = new Key("F7");
    public static readonly Key F8 = new Key("F8");
    public static readonly Key F9 = new Key("F9");
    public static readonly Key F10 = new Key("F10");
    public static readonly Key F11 = new Key("F11");
    public static readonly Key F12 = new Key("F12");
    public static readonly Key F13 = new Key("F13");
    public static readonly Key F14 = new Key("F14");
    public static readonly Key F15 = new Key("F15");
    public static readonly Key F16 = new Key("F16");
    public static readonly Key F17 = new Key("F17");
    public static readonly Key F18 = new Key("F18");
    public static readonly Key F19 = new Key("F19");
    public static readonly Key F20 = new Key("F20");
    public static readonly Key F21 = new Key("F21");
    public static readonly Key F22 = new Key("F22");
    public static readonly Key F23 = new Key("F23");
    public static readonly Key F24 = new Key("F24");

    public static readonly Key ContextMenu = Info;

    #endregion

    #region Protected fields

    /// <summary>
    /// Contains the raw code.
    /// </summary>
    protected char? _rawCode;

    /// <summary>
    /// Contains the key name.
    /// </summary>
    protected string _name;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a special key which has no character code.
    /// </summary>
    /// <param name="name">Name of the special key.</param>
    public Key(string name)
    {
      ILogger logger = ServiceRegistration.Get<ILogger>();
      _name = name;
      if (NAME2SPECIALKEY.ContainsKey(name))
        logger.Warn("Key: Special key '{0}' was instantiated multiple times", name);
      else
        NAME2SPECIALKEY.Add(name, this);
    }

    /// <summary>
    /// Creates a printable key (alpha-numeric or special character).
    /// </summary>
    /// <param name="rawCode">Char code of the key.</param>
    public Key(char rawCode)
    {
      _rawCode = rawCode;
      _name = rawCode.ToString();
    }

    /// <summary>
    /// Internal constructor for XML serialization.
    /// </summary>
    internal Key() { }

    #endregion

    /// <summary>
    /// Returns the name of this key command. For printable character keys, the name will be the the
    /// character itself. For special keys, the name tells the function of the key in one word (like "Delete").
    /// </summary>
    public string Name
    {
      get { return _name; }
    }

    /// <summary>
    /// For printable key commands, this property contains the character of the key. For non-printable keys,
    /// this property doesn't have a value.
    /// </summary>
    public char? RawCode
    {
      get { return _rawCode; }
    }

    /// <summary>
    /// Returns the information if this key is a special key, i.e. doesn't have a <see cref="RawCode"/>.
    /// </summary>
    public bool IsSpecialKey
    {
      get { return !_rawCode.HasValue; }
    }

    /// <summary>
    /// Returns the information if this key is a printable key, i.e. contains a <see cref="RawCode"/>.
    /// </summary>
    public bool IsPrintableKey
    {
      get { return _rawCode.HasValue; }
    }

    public static Key GetSpecialKeyByName(string name)
    {
      Key result;
      return NAME2SPECIALKEY.TryGetValue(name, out result) ? result : null;
    }

    public static Key Printable(char rawCode)
    {
      return new Key(rawCode);
    }

    #region Base overrides

    public override string ToString()
    {
      return string.Format("[{0}]", _name);
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      Key key = obj as Key;
      if (key == null)
        return false;
      return key._name == _name;
    }

    public static bool operator ==(Key c1, Key c2)
    {
      bool c2null = ReferenceEquals(c2, null);
      if (ReferenceEquals(c1, null))
        return c2null;
      if (c2null)
        return false;
      return c1._name == c2._name;
    }

    public static bool operator !=(Key c1, Key c2)
    {
      return !(c1 == c2);
    }

    #endregion

    public XmlSchema GetSchema ()
    {
      return null;
    }

    public void ReadXml (XmlReader reader)
    {
      string value = reader.ReadElementString();
      string codeOrName = value.Substring(2);
      if (value.StartsWith("P:"))
      {
        _rawCode = codeOrName.ToCharArray()[0];
        _name = _rawCode.ToString();
      }
      else if (value.StartsWith("S:"))
        _name = codeOrName;
    }

    public void WriteXml (XmlWriter writer)
    {
      string result = IsPrintableKey ? "P:" + RawCode : "S:" + Name;
      writer.WriteString(result);
    }
  }
}
