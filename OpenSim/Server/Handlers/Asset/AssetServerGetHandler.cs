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

using OpenSim.Framework;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Server.Base;
using OpenSim.Services.Interfaces;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace OpenSim.Server.Handlers.Asset
{
    public class AssetServerGetHandler : BaseStreamHandler
    {
        // private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IAssetService m_AssetService;

        public AssetServerGetHandler(IAssetService service) :
            base("GET", "/assets")
        {
            m_AssetService = service;
        }

        protected override byte[] ProcessRequest(string path, Stream request,
                IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            byte[] result = new byte[0];

            string[] p = SplitParams(path);

            if (p.Length == 0)
                return result;

            if (p.Length > 1)
            {
                string id = p[0];
                string cmd = p[1];

                if (cmd == "data")
                {
                    result = m_AssetService.GetData(id);
                    if (result == null)
                    {
                        httpResponse.StatusCode = (int)HttpStatusCode.NotFound;
                        httpResponse.ContentType = "text/plain";
                        result = new byte[0];
                    }
                    else
                    {
                        httpResponse.StatusCode = (int)HttpStatusCode.OK;
                        httpResponse.ContentType = "application/octet-stream";
                    }
                }
                else if (cmd == "metadata")
                {
                    AssetMetadata metadata = m_AssetService.GetMetadata(id);

                    if (metadata != null)
                    {
                        XmlSerializer xs =
                                new XmlSerializer(typeof(AssetMetadata));
                        result = ServerUtils.SerializeResult(xs, metadata);

                        httpResponse.StatusCode = (int)HttpStatusCode.OK;
                        httpResponse.ContentType =
                                SLUtil.SLAssetTypeToContentType(metadata.Type);
                    }
                    else
                    {
                        httpResponse.StatusCode = (int)HttpStatusCode.NotFound;
                        httpResponse.ContentType = "text/plain";
                        result = new byte[0];
                    }
                }
                else
                {
                    // Unknown request
                    httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    httpResponse.ContentType = "text/plain";
                    result = new byte[0];
                }
            }
            else if (p.Length == 1)
            {
                // Get the entire asset (metadata + data)

                string id = p[0];
                AssetBase asset = m_AssetService.Get(id);

                if (asset != null)
                {
                    XmlSerializer xs = new XmlSerializer(typeof(AssetBase));
                    result = ServerUtils.SerializeResult(xs, asset);

                    httpResponse.StatusCode = (int)HttpStatusCode.OK;
                    httpResponse.ContentType =
                            SLUtil.SLAssetTypeToContentType(asset.Type);
                }
                else
                {
                    httpResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    httpResponse.ContentType = "text/plain";
                    result = new byte[0];
                }
            }
            else
            {
                // Unknown request
                httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                httpResponse.ContentType = "text/plain";
                result = new byte[0];
            }

            return result;
        }
    }
}