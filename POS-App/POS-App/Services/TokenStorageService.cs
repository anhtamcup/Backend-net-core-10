using CredentialManagement;
using POS_App.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace POS_App.Services
{
    public static class TokenStorageService
    {
        private const string KeyAccess = "POSApp_AccessToken";
        private const string KeyRefresh = "POSApp_RefreshToken";
        private const string KeySession = "POSApp_SessionId";
        private const string KeyExpiry = "POSApp_ExpiresAt";

        public static void Save(AuthSession session)
        {
            Set(KeyAccess, session.AccessToken);
            Set(KeyRefresh, session.RefreshToken);
            Set(KeySession, session.SessionId);
            Set(KeyExpiry, session.ExpiresAt.ToString("o"));
        }

        public static AuthSession Load()
        {
            var access = Get(KeyAccess);
            if (access == null) return null;

            return new AuthSession
            {
                AccessToken = access,
                RefreshToken = Get(KeyRefresh),
                SessionId = Get(KeySession),
                ExpiresAt = DateTime.TryParse(Get(KeyExpiry), out var dt)
                               ? dt : DateTime.MinValue
            };
        }

        public static void Clear()
        {
            Delete(KeyAccess);
            Delete(KeyRefresh);
            Delete(KeySession);
            Delete(KeyExpiry);
        }

        public static bool IsExpired()
        {
            var session = Load();
            if (session == null) return true;
            return DateTime.UtcNow >= session.ExpiresAt.AddMinutes(-5);
        }

        // ── helpers ──────────────────────────────────────────
        private static void Set(string target, string value)
        {
            new Credential
            {
                Target = target,
                Username = "pos_app",
                Password = value ?? "",
                Type = CredentialType.Generic,
                PersistanceType = PersistanceType.LocalComputer
            }.Save();
        }

        private static string Get(string target)
        {
            var c = new Credential { Target = target };
            return c.Load() ? c.Password : null;
        }

        private static void Delete(string target)
        {
            var c = new Credential { Target = target };
            if (c.Load()) c.Delete();
        }
    }
}
