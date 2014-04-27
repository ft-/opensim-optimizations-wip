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

using Nini.Config;
using OpenMetaverse;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Region.ScriptEngine.Shared;
using System.Reflection;

namespace OpenSim.Region.ScriptEngine.Interfaces
{
    /// <summary>
    /// An interface for a script API module to communicate with
    /// the engine it's running under
    /// </summary>
    public interface IScriptEngine
    {
        IConfig Config { get; }

        IConfigSource ConfigSource { get; }

        /// <summary>
        /// Return the name of the base class that will be used for all running scripts.
        /// </summary>
        string ScriptBaseClassName { get; }

        /// <summary>
        /// Parameters for the generated script's constructor.
        /// </summary>
        /// <remarks>
        /// Can be null if there are no parameters
        /// </remarks>
        ParameterInfo[] ScriptBaseClassParameters { get; }

        /// <summary>
        /// Return the name of the class that will be used for all running scripts.
        /// </summary>
        /// <remarks>
        /// Each class goes in its own assembly so we don't need to otherwise distinguish the class name.
        /// </remarks>
        string ScriptClassName { get; }

        string ScriptEngineName { get; }

        string ScriptEnginePath { get; }

        IScriptModule ScriptModule { get; }

        /// <summary>
        /// Assemblies that need to be referenced when compiling scripts.
        /// </summary>
        /// <remarks>
        /// These are currently additional to those always referenced by the compiler, BUT THIS MAY CHANGE IN THE
        /// FUTURE.
        /// This can be null if there are no additional assemblies.
        /// </remarks>
        string[] ScriptReferencedAssemblies { get; }

        Scene World { get; }

        void ApiResetScript(UUID itemID);

        IScriptApi GetApi(UUID itemID, string name);

        DetectParams GetDetectParams(UUID item, int number);

        bool GetScriptState(UUID itemID);

        int GetStartParameter(UUID itemID);

        /// <summary>
        /// Post event to an entire prim
        /// </summary>
        bool PostObjectEvent(uint localID, EventParams parms);

        /// <summary>
        /// Post an event to a single script
        /// </summary>
        bool PostScriptEvent(UUID itemID, EventParams parms);

        /// <summary>
        /// Queue an event for execution
        /// </summary>
        IScriptWorkItem QueueEventHandler(object parms);
        void ResetScript(UUID itemID);

        void SetMinEventDelay(UUID itemID, double delay);
        void SetScriptState(UUID itemID, bool state);
        void SetState(UUID itemID, string newState);
    }
}