using System;
using System.Collections.Generic;
using System.Linq;
using File = System.IO.File;

namespace ImageBoard
{
    public class SavedSettings
    {
        private readonly List<long> _savedChatIds;

        public SavedSettings()
        {
            _savedChatIds = new List<long>();
            try
            {
                var text = File.ReadAllText("/data/chats.bot");
                _savedChatIds = text.Split(" ").Select(long.Parse).ToList();
            }
            catch (Exception ex)
            {
                
            }
        }

        public IEnumerable<long> SavedChatIds => _savedChatIds;

        public void AddChat(long id)
        {
            _savedChatIds.Add(id);
            File.WriteAllText("/data/chats.bot", string.Join(" ", _savedChatIds));
        }
    }
}