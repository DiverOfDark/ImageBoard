using System;
using System.Collections.Generic;
using System.Linq;
using File = System.IO.File;

namespace ImageBoard
{
    public class SavedSettings
    {
        private readonly List<long> _savedChatIds;
        private readonly List<String> _savedAdmins;

        public SavedSettings()
        {
            _savedChatIds = new List<long>();
            _savedAdmins = new List<string> {"diverofdark"};
            try
            {
                var text = File.ReadAllText("/data/chats.bot");
                _savedChatIds = text.Split(" ").Select(long.Parse).ToList();
            }
            catch (Exception ex)
            {
            }

            try
            {
                var admins = File.ReadAllLines("/data/admins.bot");
                _savedAdmins = admins.Select(v => v.Trim()).ToList();
            }
            catch (Exception ex)
            {
            }
        }

        public IEnumerable<long> SavedChatIds => _savedChatIds;
        public IEnumerable<String> Admins => _savedAdmins;

        public void AddChat(long id)
        {
            if (!_savedChatIds.Contains(id))
            {
                _savedChatIds.Add(id);
                File.WriteAllText("/data/chats.bot", string.Join(" ", _savedChatIds));
            }
        }

        public void AddAdmin(string text)
        {
            if (!_savedAdmins.Contains(text))
            {
                _savedAdmins.Add(text);
                File.WriteAllText("/data/admins.bot", string.Join("\n", _savedAdmins));
            }
        }
    }
}