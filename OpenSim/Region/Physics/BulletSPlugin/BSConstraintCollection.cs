﻿/*
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
    public sealed class BSConstraintCollection : IDisposable
    {
        // private static readonly ILog m_log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // private static readonly string LogHeader = "[CONSTRAINT COLLECTION]";

        private List<BSConstraint> m_constraints;
        private ReaderWriterLock m_constraintsRwLock = new ReaderWriterLock();

        private BulletWorld m_world;

        public BSConstraintCollection(BulletWorld world)
        {
            m_world = world;
            m_constraints = new List<BSConstraint>();
        }

        private delegate bool ConstraintAction(BSConstraint constrain);

        public bool AddConstraint(BSConstraint cons)
        {
            m_constraintsRwLock.AcquireWriterLock(-1);
            try
            {
                // There is only one constraint between any bodies. Remove any old just to make sure.
                RemoveAndDestroyConstraint(cons.Body1, cons.Body2);

                m_constraints.Add(cons);
            }
            finally
            {
                m_constraintsRwLock.ReleaseWriterLock();
            }

            return true;
        }

        public void Clear()
        {
            m_constraintsRwLock.AcquireWriterLock(-1);
            try
            {
                foreach (BSConstraint cons in m_constraints)
                {
                    cons.Dispose();
                }
                m_constraints.Clear();
            }
            finally
            {
                m_constraintsRwLock.ReleaseWriterLock();
            }
        }

        public void Dispose()
        {
            this.Clear();
        }

        public bool RecalculateAllConstraints()
        {
            bool ret = false;
            m_constraintsRwLock.AcquireReaderLock(-1);
            try
            {
                foreach (BSConstraint constrain in m_constraints)
                {
                    constrain.CalculateTransforms();
                    ret = true;
                }
            }
            finally
            {
                m_constraintsRwLock.ReleaseReaderLock();
            }
            return ret;
        }

        // Remove any constraint between the passed bodies.
        // Presumed there is only one such constraint possible.
        // Return 'true' if a constraint was found and destroyed.
        public bool RemoveAndDestroyConstraint(BulletBody body1, BulletBody body2)
        {
            bool ret = false;
            m_constraintsRwLock.AcquireWriterLock(-1);
            try
            {
                BSConstraint constrain;
                if (this.TryGetConstraint(body1, body2, out constrain))
                {
                    // remove the constraint from our collection
                    ret = RemoveAndDestroyConstraint(constrain);
                }
            }
            finally
            {
                m_constraintsRwLock.ReleaseWriterLock();
            }

            return ret;
        }

        // The constraint MUST exist in the collection
        // Could be called if the constraint was previously removed.
        // Return 'true' if the constraint was actually removed and disposed.
        public bool RemoveAndDestroyConstraint(BSConstraint constrain)
        {
            bool removed = false;
            m_constraintsRwLock.AcquireWriterLock(-1);
            try
            {
                // remove the constraint from our collection
                removed = m_constraints.Remove(constrain);
            }
            finally
            {
                m_constraintsRwLock.ReleaseWriterLock();
            }
            // Dispose() is safe to call multiple times
            constrain.Dispose();
            return removed;
        }

        // Remove all constraints that reference the passed body.
        // Return 'true' if any constraints were destroyed.
        public bool RemoveAndDestroyConstraint(BulletBody body1)
        {
            List<BSConstraint> toRemove = new List<BSConstraint>();
            uint lookingID = body1.ID;
            m_constraintsRwLock.AcquireWriterLock(-1);
            try
            {
                foreach (BSConstraint constrain in m_constraints)
                {
                    if (constrain.Body1.ID == lookingID || constrain.Body2.ID == lookingID)
                    {
                        toRemove.Add(constrain);
                    }
                }
                foreach (BSConstraint constrain in toRemove)
                {
                    m_constraints.Remove(constrain);
                    constrain.Dispose();
                }
            }
            finally
            {
                m_constraintsRwLock.ReleaseWriterLock();
            }
            return (toRemove.Count > 0);
        }

        // Get the constraint between two bodies. There can be only one.
        // Return 'true' if a constraint was found.
        public bool TryGetConstraint(BulletBody body1, BulletBody body2, out BSConstraint returnConstraint)
        {
            bool found = false;
            BSConstraint foundConstraint = null;

            uint lookingID1 = body1.ID;
            uint lookingID2 = body2.ID;
            m_constraintsRwLock.AcquireReaderLock(-1);
            try
            {
                foreach (BSConstraint constrain in m_constraints)
                {
                    if ((constrain.Body1.ID == lookingID1 && constrain.Body2.ID == lookingID2)
                        || (constrain.Body1.ID == lookingID2 && constrain.Body2.ID == lookingID1))
                    {
                        foundConstraint = constrain;
                        found = true;
                        break;
                    }
                }
            }
            finally
            {
                m_constraintsRwLock.ReleaseReaderLock();
            }
            returnConstraint = foundConstraint;
            return found;
        }
    }
}