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

namespace OpenSim.Addons.XmlRpcGroups
{
    public interface IGroupsServicesConnector
    {
        void AddAgentToGroup(UUID RequestingAgentID, UUID AgentID, UUID GroupID, UUID RoleID);

        void AddAgentToGroupInvite(UUID RequestingAgentID, UUID inviteID, UUID groupID, UUID roleID, UUID agentID);

        void AddAgentToGroupRole(UUID RequestingAgentID, UUID AgentID, UUID GroupID, UUID RoleID);

        void AddGroupNotice(UUID RequestingAgentID, UUID groupID, UUID noticeID, string fromName, string subject, string message, byte[] binaryBucket);

        void AddGroupRole(UUID RequestingAgentID, UUID groupID, UUID roleID, string name, string description, string title, ulong powers);

        void AgentDroppedFromGroupChatSession(UUID agentID, UUID groupID);

        void AgentInvitedToGroupChatSession(UUID agentID, UUID groupID);

        UUID CreateGroup(UUID RequestingAgentID, string name, string charter, bool showInList, UUID insigniaID, int membershipFee, bool openEnrollment, bool allowPublish, bool maturePublish, UUID founderID);

        List<DirGroupsReplyData> FindGroups(UUID RequestingAgentID, string search);

        GroupMembershipData GetAgentActiveMembership(UUID RequestingAgentID, UUID AgentID);

        /// <summary>
        /// Get information about a specific group to which the user belongs.
        /// </summary>
        /// <param name="RequestingAgentID">The agent requesting the information.</param>
        /// <param name="AgentID">The agent requested.</param>
        /// <param name="GroupID">The group requested.</param>
        /// <returns>
        /// If the user is a member of the group then the data structure is returned.  If not, then null is returned.
        /// </returns>
        GroupMembershipData GetAgentGroupMembership(UUID RequestingAgentID, UUID AgentID, UUID GroupID);

        /// <summary>
        /// Get information about the groups to which a user belongs.
        /// </summary>
        /// <param name="RequestingAgentID">The agent requesting the information.</param>
        /// <param name="AgentID">The agent requested.</param>
        /// <returns>
        /// Information about the groups to which the user belongs.  If the user belongs to no groups then an empty
        /// list is returned.
        /// </returns>
        List<GroupMembershipData> GetAgentGroupMemberships(UUID RequestingAgentID, UUID AgentID);

        List<GroupRolesData> GetAgentGroupRoles(UUID RequestingAgentID, UUID AgentID, UUID GroupID);

        GroupInviteInfo GetAgentToGroupInvite(UUID RequestingAgentID, UUID inviteID);

        List<GroupMembersData> GetGroupMembers(UUID RequestingAgentID, UUID GroupID);

        GroupNoticeInfo GetGroupNotice(UUID RequestingAgentID, UUID noticeID);

        List<GroupNoticeData> GetGroupNotices(UUID RequestingAgentID, UUID GroupID);

        /// <summary>
        /// Get the group record.
        /// </summary>
        /// <returns></returns>
        /// <param name='RequestingAgentID'>The UUID of the user making the request.</param>
        /// <param name='GroupID'>
        /// The ID of the record to retrieve.
        /// GroupName may be specified instead, in which case this parameter will be UUID.Zero
        /// </param>
        /// <param name='GroupName'>
        /// The name of the group to retrieve.
        /// GroupID may be specified instead, in which case this parmeter will be null.
        /// </param>
        GroupRecord GetGroupRecord(UUID RequestingAgentID, UUID GroupID, string GroupName);

        List<GroupRoleMembersData> GetGroupRoleMembers(UUID RequestingAgentID, UUID GroupID);

        List<GroupRolesData> GetGroupRoles(UUID RequestingAgentID, UUID GroupID);

        bool hasAgentBeenInvitedToGroupChatSession(UUID agentID, UUID groupID);

        bool hasAgentDroppedGroupChatSession(UUID agentID, UUID groupID);

        void RemoveAgentFromGroup(UUID RequestingAgentID, UUID AgentID, UUID GroupID);

        void RemoveAgentFromGroupRole(UUID RequestingAgentID, UUID AgentID, UUID GroupID, UUID RoleID);

        void RemoveAgentToGroupInvite(UUID RequestingAgentID, UUID inviteID);

        void RemoveGroupRole(UUID RequestingAgentID, UUID groupID, UUID roleID);

        void ResetAgentGroupChatSessions(UUID agentID);

        void SetAgentActiveGroup(UUID RequestingAgentID, UUID AgentID, UUID GroupID);

        void SetAgentActiveGroupRole(UUID RequestingAgentID, UUID AgentID, UUID GroupID, UUID RoleID);

        void SetAgentGroupInfo(UUID RequestingAgentID, UUID AgentID, UUID GroupID, bool AcceptNotices, bool ListInProfile);

        void UpdateGroup(UUID RequestingAgentID, UUID groupID, string charter, bool showInList, UUID insigniaID, int membershipFee, bool openEnrollment, bool allowPublish, bool maturePublish);

        void UpdateGroupRole(UUID RequestingAgentID, UUID groupID, UUID roleID, string name, string description, string title, ulong powers);
    }

    public class GroupInviteInfo
    {
        public UUID AgentID = UUID.Zero;
        public UUID GroupID = UUID.Zero;
        public UUID InviteID = UUID.Zero;
        public UUID RoleID = UUID.Zero;
    }
}