using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace LifeCraft.Systems
{
    /// <summary>
    /// Manages the player's friends list with premium unlimited friends feature
    /// </summary>
    public class FriendsManager : MonoBehaviour
    {
        [Header("Friends List Settings")]
        [SerializeField] private int maxFreeFriends = 100; // Freemium limit

        [Header("Events")]
        public UnityEvent<List<FriendData>> OnFriendsListUpdated;
        public UnityEvent<FriendData> OnFriendAdded;
        public UnityEvent<FriendData> OnFriendRemoved;
        public UnityEvent<string> OnFriendsLimitReached;

        // Friends data
        private List<FriendData> friendsList = new List<FriendData>();

        // Singleton pattern
        public static FriendsManager Instance { get; private set; }

        [System.Serializable]
        public class FriendData
        {
            public string friendId;
            public string displayName;
            public string username;
            public string avatarUrl;
            public DateTime dateAdded;
            public bool isOnline;
            public DateTime lastSeen;
            public FriendStatus status;
        }

        public enum FriendStatus
        {
            Pending,    // Friend request sent, waiting for response
            Accepted,   // Friend request accepted
            Blocked,    // Friend blocked
            Removed     // Friend removed
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadFriendsList();
        }

        /// <summary>
        /// Add a new friend to the list
        /// </summary>
        public bool AddFriend(string friendId, string displayName, string username = "", string avatarUrl = "")
        {
            // Check if friend already exists
            if (friendsList.Any(f => f.friendId == friendId))
            {
                Debug.LogWarning($"Friend {friendId} already exists in friends list");
                return false;
            }

            // Check friends limit for freemium users
            if (!CanAddMoreFriends())
            {
                OnFriendsLimitReached?.Invoke($"You've reached the limit of {maxFreeFriends} friends. Upgrade to Premium for unlimited friends!");
                return false;
            }

            var newFriend = new FriendData
            {
                friendId = friendId,
                displayName = displayName,
                username = username,
                avatarUrl = avatarUrl,
                dateAdded = DateTime.Now,
                isOnline = false,
                lastSeen = DateTime.Now,
                status = FriendStatus.Accepted
            };

            friendsList.Add(newFriend);
            SaveFriendsList();
            
            OnFriendAdded?.Invoke(newFriend);
            OnFriendsListUpdated?.Invoke(new List<FriendData>(friendsList));
            
            Debug.Log($"Added friend: {displayName} ({friendId})");
            return true;
        }

        /// <summary>
        /// Remove a friend from the list
        /// </summary>
        public bool RemoveFriend(string friendId)
        {
            var friend = friendsList.FirstOrDefault(f => f.friendId == friendId);
            if (friend != null)
            {
                friendsList.Remove(friend);
                SaveFriendsList();
                
                OnFriendRemoved?.Invoke(friend);
                OnFriendsListUpdated?.Invoke(new List<FriendData>(friendsList));
                
                Debug.Log($"Removed friend: {friend.displayName} ({friendId})");
                return true;
            }
            
            Debug.LogWarning($"Friend {friendId} not found in friends list");
            return false;
        }

        /// <summary>
        /// Check if user can add more friends
        /// </summary>
        public bool CanAddMoreFriends()
        {
            // Premium users have unlimited friends
            if (SubscriptionManager.Instance != null && SubscriptionManager.Instance.HasUnlimitedFriends())
            {
                return true;
            }

            // Freemium users are limited
            return friendsList.Count < maxFreeFriends;
        }

        /// <summary>
        /// Get current friends count
        /// </summary>
        public int GetFriendsCount()
        {
            return friendsList.Count;
        }

        /// <summary>
        /// Get maximum friends allowed
        /// </summary>
        public int GetMaxFriends()
        {
            if (SubscriptionManager.Instance != null && SubscriptionManager.Instance.HasUnlimitedFriends())
            {
                return int.MaxValue; // Unlimited
            }
            return maxFreeFriends;
        }

        /// <summary>
        /// Get all friends
        /// </summary>
        public List<FriendData> GetAllFriends()
        {
            return new List<FriendData>(friendsList);
        }

        /// <summary>
        /// Get online friends
        /// </summary>
        public List<FriendData> GetOnlineFriends()
        {
            return friendsList.Where(f => f.isOnline).ToList();
        }

        /// <summary>
        /// Update friend's online status
        /// </summary>
        public void UpdateFriendStatus(string friendId, bool isOnline)
        {
            var friend = friendsList.FirstOrDefault(f => f.friendId == friendId);
            if (friend != null)
            {
                friend.isOnline = isOnline;
                friend.lastSeen = DateTime.Now;
                SaveFriendsList();
                
                OnFriendsListUpdated?.Invoke(new List<FriendData>(friendsList));
            }
        }

        /// <summary>
        /// Send friend request
        /// </summary>
        public bool SendFriendRequest(string friendId, string displayName)
        {
            // Check if friend already exists
            if (friendsList.Any(f => f.friendId == friendId))
            {
                Debug.LogWarning($"Friend {friendId} already exists in friends list");
                return false;
            }

            // Check friends limit for freemium users
            if (!CanAddMoreFriends())
            {
                OnFriendsLimitReached?.Invoke($"You've reached the limit of {maxFreeFriends} friends. Upgrade to Premium for unlimited friends!");
                return false;
            }

            var pendingFriend = new FriendData
            {
                friendId = friendId,
                displayName = displayName,
                dateAdded = DateTime.Now,
                isOnline = false,
                lastSeen = DateTime.Now,
                status = FriendStatus.Pending
            };

            friendsList.Add(pendingFriend);
            SaveFriendsList();
            
            OnFriendAdded?.Invoke(pendingFriend);
            OnFriendsListUpdated?.Invoke(new List<FriendData>(friendsList));
            
            Debug.Log($"Sent friend request to: {displayName} ({friendId})");
            return true;
        }

        /// <summary>
        /// Accept friend request
        /// </summary>
        public bool AcceptFriendRequest(string friendId)
        {
            var friend = friendsList.FirstOrDefault(f => f.friendId == friendId && f.status == FriendStatus.Pending);
            if (friend != null)
            {
                friend.status = FriendStatus.Accepted;
                SaveFriendsList();
                
                OnFriendsListUpdated?.Invoke(new List<FriendData>(friendsList));
                
                Debug.Log($"Accepted friend request from: {friend.displayName} ({friendId})");
                return true;
            }
            
            Debug.LogWarning($"Friend request from {friendId} not found");
            return false;
        }

        /// <summary>
        /// Block a friend
        /// </summary>
        public bool BlockFriend(string friendId)
        {
            var friend = friendsList.FirstOrDefault(f => f.friendId == friendId);
            if (friend != null)
            {
                friend.status = FriendStatus.Blocked;
                SaveFriendsList();
                
                OnFriendsListUpdated?.Invoke(new List<FriendData>(friendsList));
                
                Debug.Log($"Blocked friend: {friend.displayName} ({friendId})");
                return true;
            }
            
            Debug.LogWarning($"Friend {friendId} not found in friends list");
            return false;
        }

        /// <summary>
        /// Unblock a friend
        /// </summary>
        public bool UnblockFriend(string friendId)
        {
            var friend = friendsList.FirstOrDefault(f => f.friendId == friendId && f.status == FriendStatus.Blocked);
            if (friend != null)
            {
                friend.status = FriendStatus.Accepted;
                SaveFriendsList();
                
                OnFriendsListUpdated?.Invoke(new List<FriendData>(friendsList));
                
                Debug.Log($"Unblocked friend: {friend.displayName} ({friendId})");
                return true;
            }
            
            Debug.LogWarning($"Blocked friend {friendId} not found");
            return false;
        }

        /// <summary>
        /// Get pending friend requests
        /// </summary>
        public List<FriendData> GetPendingRequests()
        {
            return friendsList.Where(f => f.status == FriendStatus.Pending).ToList();
        }

        /// <summary>
        /// Get blocked friends
        /// </summary>
        public List<FriendData> GetBlockedFriends()
        {
            return friendsList.Where(f => f.status == FriendStatus.Blocked).ToList();
        }

        /// <summary>
        /// Save friends list to PlayerPrefs
        /// </summary>
        private void SaveFriendsList()
        {
            try
            {
                var saveData = new FriendsListSaveData
                {
                    friends = friendsList
                };
                
                string json = JsonUtility.ToJson(saveData);
                PlayerPrefs.SetString("FriendsListData", json);
                PlayerPrefs.Save();
                
                Debug.Log($"Saved friends list: {friendsList.Count} friends");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save friends list: {e.Message}");
            }
        }

        /// <summary>
        /// Load friends list from PlayerPrefs
        /// </summary>
        private void LoadFriendsList()
        {
            try
            {
                if (PlayerPrefs.HasKey("FriendsListData"))
                {
                    string json = PlayerPrefs.GetString("FriendsListData");
                    var saveData = JsonUtility.FromJson<FriendsListSaveData>(json);
                    
                    if (saveData != null && saveData.friends != null)
                    {
                        friendsList = saveData.friends;
                        Debug.Log($"Loaded friends list: {friendsList.Count} friends");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load friends list: {e.Message}");
                friendsList = new List<FriendData>();
            }
        }

        /// <summary>
        /// Clear all friends (for debugging)
        /// </summary>
        public void ClearFriendsList()
        {
            friendsList.Clear();
            SaveFriendsList();
            OnFriendsListUpdated?.Invoke(new List<FriendData>());
            Debug.Log("Friends list cleared");
        }

        [System.Serializable]
        private class FriendsListSaveData
        {
            public List<FriendData> friends = new List<FriendData>();
        }
    }
}
