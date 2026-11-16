using System;
using System.Collections.Generic;
using Shrine.Data;
using UnityEngine;

namespace Shrine.Core
{
    [Serializable]
    public sealed class ShrineProfile
    {
        public string Name = "Pilgrim";
        public PatronKind Patron = PatronKind.Flame;
        public List<string> Deck = new List<string>();
        public int Games;
        public int Wins;
        public int Losses;
        public int Xp;
        public int AvatarIndex;

        public int Rank => 1 + Xp / 250;

        public static ShrineProfile CreateDefault() => new ShrineProfile
        {
            Name = "Pilgrim",
            Patron = PatronKind.Flame,
            Deck = new List<string>(),
            AvatarIndex = 0
        };
    }

    /// <summary>Local, password-free profile storage. Replaces the old plaintext login.</summary>
    public static class ProfileStore
    {
        private const string KeyV2 = "Shrine.Profile.v2";
        private const string LegacyKey = "UserData";

        public static ShrineProfile Load()
        {
            string json = PlayerPrefs.GetString(KeyV2, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    ShrineProfile profile = JsonUtility.FromJson<ShrineProfile>(json);
                    if (profile != null) return profile;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Shrine profile could not be read: {e.Message}");
                }
            }
            return MigrateLegacy();
        }

        public static void Save(ShrineProfile profile)
        {
            PlayerPrefs.SetString(KeyV2, JsonUtility.ToJson(profile));
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(KeyV2);
            PlayerPrefs.Save();
        }

        /// <summary>One-time migration: discard legacy credentials, keep the display name.</summary>
        private static ShrineProfile MigrateLegacy()
        {
            ShrineProfile profile = ShrineProfile.CreateDefault();
            try
            {
                string legacy = PlayerPrefs.GetString(LegacyKey, string.Empty);
                if (!string.IsNullOrEmpty(legacy))
                {
                    var wrapper = JsonUtility.FromJson<LegacyUserList>(legacy);
                    string last = PlayerPrefs.GetString("LastLoggedInUser", string.Empty);
                    if (wrapper != null && wrapper.users != null && wrapper.users.Count > 0)
                    {
                        profile.Name = wrapper.users.Exists(u => u.Username == last)
                            ? last
                            : wrapper.users[0].Username;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Legacy profile migration skipped: {e.Message}");
            }
            PlayerPrefs.DeleteKey(LegacyKey);
            PlayerPrefs.DeleteKey("LastLoggedInUser");
            Save(profile);
            return profile;
        }

        [Serializable]
        private sealed class LegacyUserList
        {
            public List<LegacyUser> users = new List<LegacyUser>();
        }

        [Serializable]
        private sealed class LegacyUser
        {
            public string Username;
        }
    }
}
