/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyrightD
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

using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenSim.Region.Physics.BulletSPlugin
{
    // =============================================================================
    /// <summary>
    /// Each physical object can have 'actors' who are pushing the object around.
    /// This can be used for hover, locking axis, making vehicles, etc.
    /// Each physical object can have multiple actors acting on it.
    ///
    /// An actor usually registers itself with physics scene events (pre-step action)
    /// and modifies the parameters on the host physical object.
    /// </summary>
    public abstract class BSActor
    {
        public BSActor(BSScene physicsScene, BSPhysObject pObj, string actorName)
        {
            m_physicsScene = physicsScene;
            m_controllingPrim = pObj;
            ActorName = actorName;
            Enabled = true;
        }

        public string ActorName { get; private set; }

        public virtual bool Enabled { get; set; }

        // Return 'true' if activily updating the prim
        public virtual bool isActive
        {
            get { return Enabled; }
        }

        protected BSPhysObject m_controllingPrim { get; private set; }

        protected BSScene m_physicsScene { get; private set; }

        // Release any connections and resources used by the actor.
        public abstract void Dispose();

        // Called when physical parameters (properties set in Bullet) need to be re-applied.
        public abstract void Refresh();

        // The object's physical representation is being rebuilt so pick up any physical dependencies (constraints, ...).
        //     Register a prestep action to restore physical requirements before the next simulation step.
        public abstract void RemoveDependencies();

        // Turn the actor on an off. Only used by ActorCollection to set all enabled/disabled.
        // Anyone else should assign true/false to 'Enabled'.
        public void SetEnabled(bool setEnabled)
        {
            Enabled = setEnabled;
        }
    }

    public class BSActorCollection
    {
        private Dictionary<string, BSActor> m_actors;
        private ReaderWriterLock m_actorsRwLock = new ReaderWriterLock();

        public BSActorCollection(BSScene physicsScene)
        {
            m_physicsScene = physicsScene;
            m_actors = new Dictionary<string, BSActor>();
        }

        private BSScene m_physicsScene { get; set; }

        public void Add(string name, BSActor actor)
        {
            m_actorsRwLock.AcquireWriterLock(-1);
            try
            {
                if (!m_actors.ContainsKey(name))
                {
                    m_actors[name] = actor;
                }
            }
            finally
            {
                m_actorsRwLock.ReleaseWriterLock();
            }
        }

        public void Clear()
        {
            m_actorsRwLock.AcquireWriterLock(-1);
            try
            {
                ForEachActor(a => a.Dispose());
                m_actors.Clear();
            }
            finally
            {
                m_actorsRwLock.ReleaseWriterLock();
            }
        }

        public void Dispose()
        {
            Clear();
        }

        public void Enable(bool enabl)
        {
            ForEachActor(a => a.SetEnabled(enabl));
        }

        public void ForEachActor(Action<BSActor> act)
        {
            m_actorsRwLock.AcquireReaderLock(-1);
            try
            {
                foreach (KeyValuePair<string, BSActor> kvp in m_actors)
                    act(kvp.Value);
            }
            finally
            {
                m_actorsRwLock.ReleaseReaderLock();
            }
        }

        public bool HasActor(string name)
        {
            m_actorsRwLock.AcquireReaderLock(-1);
            try
            {
                return m_actors.ContainsKey(name);
            }
            finally
            {
                m_actorsRwLock.ReleaseReaderLock();
            }
        }

        public void Refresh()
        {
            ForEachActor(a => a.Refresh());
        }

        public bool RemoveAndRelease(string name)
        {
            bool ret = false;
            m_actorsRwLock.AcquireWriterLock(-1);
            try
            {
                if (m_actors.ContainsKey(name))
                {
                    BSActor beingRemoved = m_actors[name];
                    m_actors.Remove(name);
                    beingRemoved.Dispose();
                    ret = true;
                }
            }
            finally
            {
                m_actorsRwLock.ReleaseWriterLock();
            }
            return ret;
        }

        public void RemoveDependencies()
        {
            ForEachActor(a => a.RemoveDependencies());
        }

        public bool TryGetActor(string actorName, out BSActor theActor)
        {
            m_actorsRwLock.AcquireReaderLock(-1);
            try
            {
                return m_actors.TryGetValue(actorName, out theActor);
            }
            finally
            {
                m_actorsRwLock.ReleaseReaderLock();
            }
        }
    }
}