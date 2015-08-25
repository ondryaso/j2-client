using System;
using System.Linq;
using System.Windows.Input;

namespace SIClient
{
    internal partial class Settings
    {
        public ModifierKeys GetModifier(String keys)
        {
            ModifierKeys ret = ModifierKeys.None;
            ParseModifiers(keys).ToList().ForEach(k => ret |= k);
            return ret;
        }

        public ModifierKeys[] ParseModifiers(String keys)
        {
            var parts = keys.Split('+');
            return parts.Take(parts.Length - 1).Select(k => (ModifierKeys)Enum.Parse(typeof(ModifierKeys), k)).ToArray();
        }

        public Key GetKey(String keys)
        {
            return (Key)Enum.Parse(typeof(Key), keys.Split('+').Last());
        }

        public void SaveKeys(ModifierKeys[] modifiers, Key key, ref String to)
        {
            String s = "";
            modifiers.ToList().ForEach(k => s += k.ToString() + "+");
            s += key.ToString();
            to = s;
        }
    }
}