/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using OpenMetaverse;
using OpenSim.Framework;
using System.Collections.Generic;

namespace OpenSim.Services.Interfaces
{
    /// <summary>
    /// Callback used when a user's inventory is received from the inventory service
    /// </summary>
    public delegate void InventoryReceiptCallback(
        ICollection<InventoryFolderImpl> folders, ICollection<InventoryItemBase> items);

    public interface IInventoryService
    {
        /// <summary>
        /// Add a new folder to the user's inventory
        /// </summary>
        /// <param name="folder"></param>
        /// <returns>true if the folder was successfully added</returns>
        bool AddFolder(InventoryFolderBase folder);

        /// <summary>
        /// Add a new item to the user's inventory
        /// </summary>
        /// <param name="item">
        /// The item to be added.  If item.FolderID == UUID.Zero then the item is added to the most suitable system
        /// folder.  If there is no suitable folder then the item is added to the user's root inventory folder.
        /// </param>
        /// <returns>true if the item was successfully added, false if it was not</returns>
        bool AddItem(InventoryItemBase item);

        /// <summary>
        /// Create the entire inventory for a given user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool CreateUserInventory(UUID user);

        /// <summary>
        /// Delete an item from the user's inventory
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if the item was successfully deleted</returns>
        //bool DeleteItem(InventoryItemBase item);
        bool DeleteFolders(UUID userID, List<UUID> folderIDs);

        /// <summary>
        /// Delete an item from the user's inventory
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if the item was successfully deleted</returns>
        //bool DeleteItem(InventoryItemBase item);
        bool DeleteItems(UUID userID, List<UUID> itemIDs);

        /// <summary>
        /// Get the active gestures of the agent.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<InventoryItemBase> GetActiveGestures(UUID userId);

        /// <summary>
        /// Get a folder, given by its UUID
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        InventoryFolderBase GetFolder(InventoryFolderBase folder);

        /// <summary>
        /// Gets everything (folders and items) inside a folder
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="folderID"></param>
        /// <returns>Inventory content.  null if the request failed.</returns>
        InventoryCollection GetFolderContent(UUID userID, UUID folderID);

        /// <summary>
        /// Gets the user folder for the given folder-type
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        InventoryFolderBase GetFolderForType(UUID userID, AssetType type);

        /// <summary>
        /// Gets the items inside a folder
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="folderID"></param>
        /// <returns></returns>
        List<InventoryItemBase> GetFolderItems(UUID userID, UUID folderID);

        /// <summary>
        /// Gets the skeleton of the inventory -- folders only
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<InventoryFolderBase> GetInventorySkeleton(UUID userId);

        /// <summary>
        /// Get an item, given by its UUID
        /// </summary>
        /// <param name="item"></param>
        /// <returns>null if no item was found, otherwise the found item</returns>
        InventoryItemBase GetItem(InventoryItemBase item);

        /// <summary>
        /// Retrieve the root inventory folder for the given user.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns>null if no root folder was found</returns>
        InventoryFolderBase GetRootFolder(UUID userID);

        /// <summary>
        /// Does the given user have an inventory structure?
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        bool HasInventoryForUser(UUID userID);

        /// <summary>
        /// Move an inventory folder to a new location
        /// </summary>
        /// <param name="folder">A folder containing the details of the new location</param>
        /// <returns>true if the folder was successfully moved</returns>
        bool MoveFolder(InventoryFolderBase folder);

        bool MoveItems(UUID ownerID, List<InventoryItemBase> items);

        /// <summary>
        /// Purge an inventory folder of all its items and subfolders.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns>true if the folder was successfully purged</returns>
        bool PurgeFolder(InventoryFolderBase folder);

        /// <summary>
        /// Update a folder in the user's inventory
        /// </summary>
        /// <param name="folder"></param>
        /// <returns>true if the folder was successfully updated</returns>
        bool UpdateFolder(InventoryFolderBase folder);

        /// <summary>
        /// Update an item in the user's inventory
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if the item was successfully updated</returns>
        bool UpdateItem(InventoryItemBase item);
    }
}