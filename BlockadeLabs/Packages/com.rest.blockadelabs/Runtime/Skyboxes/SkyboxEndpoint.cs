// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;
using Utilities.WebRequestRest;

namespace BlockadeLabs.Skyboxes
{
    public sealed class SkyboxEndpoint : BlockadeLabsBaseEndpoint
    {
        [Preserve]
        private class SkyboxInfoRequest
        {
            [Preserve]
            [JsonConstructor]
            public SkyboxInfoRequest([JsonProperty("request")] SkyboxInfo skyboxInfo)
            {
                SkyboxInfo = skyboxInfo;
            }

            [Preserve]
            [JsonProperty("request")]
            public SkyboxInfo SkyboxInfo { get; }
        }

        [Preserve]
        private class SkyboxOperation
        {
            [Preserve]
            [JsonConstructor]
            public SkyboxOperation(
                [JsonProperty("success")] string success,
                [JsonProperty("error")] string error)
            {
                Success = success;
                Error = error;
            }

            [Preserve]
            [JsonProperty("success")]
            public string Success { get; }

            [Preserve]
            [JsonProperty("error")]
            public string Error { get; }
        }

        public SkyboxEndpoint(BlockadeLabsClient client) : base(client) { }

        protected override string Root => string.Empty;

        /// <summary>
        /// Returns the list of predefined styles that can influence the overall aesthetic of your skybox generation.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of <see cref="SkyboxStyle"/>s.</returns>
        [Obsolete("use overload with model parameter")]
        public async Task<IReadOnlyList<SkyboxStyle>> GetSkyboxStylesAsync(CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl("skybox/styles"), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<IReadOnlyList<SkyboxStyle>>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Returns the list of predefined styles that can influence the overall aesthetic of your skybox generation.
        /// </summary>
        /// <param name="model">The <see cref="SkyboxModel"/> to get styles for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of <see cref="SkyboxStyle"/>s.</returns>
        public async Task<IReadOnlyList<SkyboxStyle>> GetSkyboxStylesAsync(SkyboxModel model, CancellationToken cancellationToken = default)
        {
            var @params = new Dictionary<string, string> { { "model_version", ((int)model).ToString() } };
            var response = await Rest.GetAsync(GetUrl("skybox/styles", @params), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<IReadOnlyList<SkyboxStyle>>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Returns the list of predefined styles that can influence the overall aesthetic of your skybox generation, sorted by style family.
        /// This route can be used in order to build a menu of styles sorted by family.
        /// </summary>
        /// <param name="model">Optional, The <see cref="SkyboxModel"/> to get styles for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of <see cref="SkyboxStyle"/>s.</returns>
        public async Task<IReadOnlyList<SkyboxStyle>> GetSkyboxStyleFamiliesAsync(SkyboxModel? model = null, CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> @params = null;

            if (model != null)
            {
                @params = new() { { "model_version", ((int)model).ToString() } };
            }

            var response = await Rest.GetAsync(GetUrl("skybox/families", @params), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<IReadOnlyList<SkyboxStyle>>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Returns the list of predefined styles for the generation menu.
        /// </summary>
        /// <param name="model">The <see cref="SkyboxModel"/> to get styles for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of <see cref="SkyboxStyle"/>s.</returns>
        internal async Task<IReadOnlyList<SkyboxStyle>> GetSkyboxStylesMenuAsync(SkyboxModel model, CancellationToken cancellationToken = default)
        {
            var @params = new Dictionary<string, string> { { "model_version", ((int)model).ToString() } };
            var response = await Rest.GetAsync(GetUrl("skybox/menu", @params), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<IReadOnlyList<SkyboxStyle>>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Generate a skybox image.
        /// </summary>
        /// <param name="skyboxRequest"><see cref="SkyboxRequest"/>.</param>
        /// <param name="exportOption">Optional, <see cref="SkyboxExportOption"/>.</param>
        /// <param name="progressCallback">Optional, <see cref="IProgress{SkyboxInfo}"/> progress callback.</param>
        /// <param name="pollingInterval">Optional, polling interval in seconds.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SkyboxInfo"/>.</returns>
        public async Task<SkyboxInfo> GenerateSkyboxAsync(SkyboxRequest skyboxRequest, SkyboxExportOption exportOption, IProgress<SkyboxInfo> progressCallback = null, int? pollingInterval = null, CancellationToken cancellationToken = default)
            => await GenerateSkyboxAsync(skyboxRequest, new[] { exportOption }, progressCallback, pollingInterval, cancellationToken);

        /// <summary>
        /// Generate a skybox image.
        /// </summary>
        /// <param name="skyboxRequest"><see cref="SkyboxRequest"/>.</param>
        /// <param name="exportOptions">Optional, <see cref="SkyboxExportOption"/>s.</param>
        /// <param name="progressCallback">Optional, <see cref="IProgress{SkyboxInfo}"/> progress callback.</param>
        /// <param name="pollingInterval">Optional, polling interval in seconds.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SkyboxInfo"/>.</returns>
        public async Task<SkyboxInfo> GenerateSkyboxAsync(SkyboxRequest skyboxRequest, SkyboxExportOption[] exportOptions = null, IProgress<SkyboxInfo> progressCallback = null, int? pollingInterval = null, CancellationToken cancellationToken = default)
        {
            var formData = new WWWForm();
            formData.AddField("prompt", skyboxRequest.Prompt);

            if (!string.IsNullOrWhiteSpace(skyboxRequest.NegativeText))
            {
                formData.AddField("negative_text", skyboxRequest.NegativeText);
            }

            if (skyboxRequest.EnhancePrompt.HasValue)
            {
                formData.AddField("enhance_prompt", skyboxRequest.EnhancePrompt.ToString());
            }

            if (skyboxRequest.Seed.HasValue)
            {
                formData.AddField("seed", skyboxRequest.Seed.Value);
            }

            if (skyboxRequest.SkyboxStyleId.HasValue)
            {
                formData.AddField("skybox_style_id", skyboxRequest.SkyboxStyleId.Value);
            }

            if (skyboxRequest.RemixImagineId.HasValue)
            {
                formData.AddField("remix_imagine_id", skyboxRequest.RemixImagineId.Value);
            }

            if (skyboxRequest.HqDepth.HasValue)
            {
                formData.AddField("return_depth_hq", skyboxRequest.HqDepth.Value.ToString());
            }

            if (skyboxRequest.ControlImage != null)
            {
                if (!string.IsNullOrWhiteSpace(skyboxRequest.ControlModel))
                {
                    formData.AddField("control_model", skyboxRequest.ControlModel);
                }

                using var imageData = new MemoryStream();
                await skyboxRequest.ControlImage.CopyToAsync(imageData, cancellationToken);
                formData.AddBinaryData("control_image", imageData.ToArray(), skyboxRequest.ControlImageFileName);
                skyboxRequest.Dispose();
            }

            var response = await Rest.PostAsync(GetUrl("skybox"), formData, parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            var skyboxInfo = JsonConvert.DeserializeObject<SkyboxInfo>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
            progressCallback?.Report(skyboxInfo);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(pollingInterval ?? 3000, CancellationToken.None).ConfigureAwait(true);
                skyboxInfo = await GetSkyboxInfoAsync(skyboxInfo, CancellationToken.None);
                progressCallback?.Report(skyboxInfo);
                if (skyboxInfo.Status is Status.Pending or Status.Processing or Status.Dispatched) { continue; }
                break;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                var cancelResult = await CancelSkyboxGenerationAsync(skyboxInfo, CancellationToken.None);

                if (!cancelResult)
                {
                    throw new Exception($"Failed to cancel generation for {skyboxInfo.Id}");
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (skyboxInfo.Status == Status.Abort)
            {
                throw new OperationCanceledException($"Generation aborted for skybox {skyboxInfo.Id}\n{skyboxInfo.ErrorMessage}\n{skyboxInfo}");
            }

            if (skyboxInfo.Status != Status.Complete)
            {
                throw new Exception($"Failed to generate skybox! {skyboxInfo.Id} -> {skyboxInfo.Status}\nError: {skyboxInfo.ErrorMessage}\n{skyboxInfo}");
            }

            skyboxInfo.SetResponseData(response, client);
            var exportTasks = new List<Task>();

            try
            {
                if (exportOptions != null)
                {
                    exportTasks.AddRange(exportOptions.Select(exportOption => ExportSkyboxAsync(skyboxInfo, exportOption, null, pollingInterval, cancellationToken)));
                }
                else
                {
                    exportTasks.Add(ExportSkyboxAsync(skyboxInfo, DefaultExportOptions.Equirectangular_PNG, null, pollingInterval, cancellationToken));
                    exportTasks.Add(ExportSkyboxAsync(skyboxInfo, DefaultExportOptions.DepthMap_PNG, null, pollingInterval, cancellationToken));
                }

                await Task.WhenAll(exportTasks).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to download skybox export!\n{e}");
            }

            skyboxInfo = await GetSkyboxInfoAsync(skyboxInfo.Id, cancellationToken);
            await skyboxInfo.LoadAssetsAsync(EnableDebug, cancellationToken);
            skyboxInfo.SetResponseData(response, client);
            return skyboxInfo;
        }

        /// <summary>
        /// Returns the skybox metadata for the given skybox id.
        /// </summary>
        /// <param name="id">Skybox Id.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SkyboxInfo"/>.</returns>
        public async Task<SkyboxInfo> GetSkyboxInfoAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl($"imagine/requests/{id}"), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            var skyboxInfo = JsonConvert.DeserializeObject<SkyboxInfoRequest>(response.Body, BlockadeLabsClient.JsonSerializationOptions).SkyboxInfo;
            skyboxInfo.SetResponseData(response, client);
            return skyboxInfo;
        }

        /// <summary>
        /// Deletes a skybox by id.
        /// </summary>
        /// <param name="id">The id of the skybox.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if skybox was successfully deleted.</returns>
        public async Task<bool> DeleteSkyboxAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl($"imagine/deleteImagine/{id}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            var skyboxOp = JsonConvert.DeserializeObject<SkyboxOperation>(response.Body, BlockadeLabsClient.JsonSerializationOptions);

            const string successStatus = "Item deleted successfully";

            if (skyboxOp is not { Success: successStatus })
            {
                throw new Exception($"Failed to delete skybox {id}!\n{skyboxOp?.Error}");
            }

            return skyboxOp.Success.Equals(successStatus);
        }

        /// <summary>
        /// Gets the previously generated skyboxes.
        /// </summary>
        /// <param name="parameters">Optional, <see cref="SkyboxHistoryParameters"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SkyboxHistory"/>.</returns>
        public async Task<SkyboxHistory> GetSkyboxHistoryAsync(SkyboxHistoryParameters parameters = null, CancellationToken cancellationToken = default)
        {
            var historyRequest = parameters ?? new SkyboxHistoryParameters();

            var @params = new Dictionary<string, string>();

            if (historyRequest.StatusFilter.HasValue)
            {
                @params.Add("status", historyRequest.StatusFilter.ToString().ToLower());
            }

            if (historyRequest.Limit.HasValue)
            {
                @params.Add("limit", historyRequest.Limit.ToString());
            }

            if (historyRequest.Offset.HasValue)
            {
                @params.Add("offset", historyRequest.Offset.ToString());
            }

            if (historyRequest.Order.HasValue)
            {
                @params.Add("order", historyRequest.Order.ToString().ToUpper());
            }

            if (historyRequest.ImagineId.HasValue)
            {
                @params.Add("imagine_id", historyRequest.ImagineId.ToString());
            }

            if (!string.IsNullOrWhiteSpace(historyRequest.QueryFilter))
            {
                @params.Add("query", UnityWebRequest.EscapeURL(historyRequest.QueryFilter));
            }

            if (!string.IsNullOrWhiteSpace(historyRequest.GeneratorFilter))
            {
                @params.Add("generator", UnityWebRequest.EscapeURL(historyRequest.GeneratorFilter));
            }

            if (historyRequest.FavoritesOnly.HasValue &&
                historyRequest.FavoritesOnly.Value)
            {
                @params.Add("my_likes", historyRequest.FavoritesOnly.Value.ToString().ToLower());
            }

            if (historyRequest.GeneratedBy.HasValue)
            {
                @params.Add("api_key_id", historyRequest.GeneratedBy.Value.ToString());
            }

            if (historyRequest.SkyboxStyleId is > 0)
            {
                @params.Add("skybox_style_id", historyRequest.SkyboxStyleId.ToString());
            }

            var response = await Rest.GetAsync(GetUrl("imagine/myRequests", @params), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            var skyboxHistory = JsonConvert.DeserializeObject<SkyboxHistory>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
            skyboxHistory.SetResponseData(response, client);
            return skyboxHistory;
        }

        /// <summary>
        /// Cancels a pending skybox generation request by id.
        /// </summary>
        /// <param name="id">The id of the skybox.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if generation was cancelled.</returns>
        public async Task<bool> CancelSkyboxGenerationAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl($"imagine/requests/{id}"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            var skyboxOp = JsonConvert.DeserializeObject<SkyboxOperation>(response.Body, BlockadeLabsClient.JsonSerializationOptions);

            if (skyboxOp is not { Success: "true" })
            {
                throw new Exception($"Failed to cancel generation for skybox {id}!\n{skyboxOp?.Error}");
            }

            return skyboxOp.Success.Equals("true");
        }

        /// <summary>
        /// Cancels ALL pending skybox generation requests.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if all generations are cancelled.</returns>
        public async Task<bool> CancelAllPendingSkyboxGenerationsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl("imagine/requests/pending"), new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            var skyboxOp = JsonConvert.DeserializeObject<SkyboxOperation>(response.Body, BlockadeLabsClient.JsonSerializationOptions);

            if (skyboxOp is not { Success: "true" })
            {
                if (skyboxOp != null &&
                    skyboxOp.Error.Contains("You don't have any pending"))
                {
                    return false;
                }

                throw new Exception($"Failed to cancel all pending skybox generations!\n{skyboxOp?.Error}");
            }

            return skyboxOp.Success.Equals("true");
        }

        /// <summary>
        /// Returns the list of all available export types.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of available export types.</returns>
        public async Task<IReadOnlyList<SkyboxExportOption>> GetAllSkyboxExportOptionsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl("skybox/export"), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<IReadOnlyList<SkyboxExportOption>>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Exports the <see cref="SkyboxInfo"/> using the provided <see cref="SkyboxExportOption"/>.
        /// </summary>
        /// <param name="skyboxInfo">Skybox to export.</param>
        /// <param name="exportOption">Export option to use.</param>
        /// <param name="progressCallback">Optional, <see cref="IProgress{SkyboxExportRequest}"/> progress callback.</param>
        /// <param name="pollingInterval">Optional, polling interval in seconds.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>Updated <see cref="SkyboxInfo"/> with exported assets loaded into memory.</returns>
        public async Task<SkyboxInfo> ExportSkyboxAsync(SkyboxInfo skyboxInfo, SkyboxExportOption exportOption, IProgress<SkyboxExportRequest> progressCallback = null, int? pollingInterval = null, CancellationToken cancellationToken = default)
        {
            var payload = $"{{\"skybox_id\":\"{skyboxInfo.ObfuscatedId}\",\"type_id\":{exportOption.Id}}}";
            var response = await Rest.PostAsync(GetUrl("skybox/export"), payload, new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            var exportRequest = JsonConvert.DeserializeObject<SkyboxExportRequest>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
            progressCallback?.Report(exportRequest);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(pollingInterval ?? 3000, CancellationToken.None).ConfigureAwait(true);
                exportRequest = await GetExportRequestStatusAsync(exportRequest, CancellationToken.None);
                progressCallback?.Report(exportRequest);
                if (exportRequest.Status is Status.Pending or Status.Processing or Status.Dispatched) { continue; }
                break;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                var cancelResult = await CancelSkyboxExportAsync(exportRequest, CancellationToken.None);

                if (!cancelResult)
                {
                    throw new Exception($"Failed to cancel export for {exportRequest.Id}");
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (exportRequest.Status == Status.Abort)
            {
                throw new OperationCanceledException($"Export aborted for skybox {skyboxInfo.Id}\n{exportRequest.ErrorMessage}\n{exportRequest}");
            }

            if (exportRequest.Status != Status.Complete)
            {
                throw new Exception($"Failed to export skybox! {exportRequest.Id} -> {exportRequest.Status}\nError: {exportRequest.ErrorMessage}\n{exportRequest}");
            }

            skyboxInfo = await GetSkyboxInfoAsync(skyboxInfo.Id, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            skyboxInfo.SetResponseData(response, client);
            return skyboxInfo;
        }

        /// <summary>
        /// Gets the status of a specified <see cref="SkyboxExportRequest"/>.
        /// </summary>
        /// <param name="exportRequest">The export option to get the current status for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>Updated <see cref="SkyboxExportRequest"/> with latest information.</returns>
        public async Task<SkyboxExportRequest> GetExportRequestStatusAsync(SkyboxExportRequest exportRequest, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl($"skybox/export/{exportRequest.Id}"), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            exportRequest = JsonConvert.DeserializeObject<SkyboxExportRequest>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
            exportRequest.SetResponseData(response, client);
            return exportRequest;
        }

        /// <summary>
        /// Cancels the specified <see cref="SkyboxExportRequest"/>.
        /// </summary>
        /// <param name="exportRequest">The export option to cancel.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if generation was cancelled.</returns>
        public async Task<bool> CancelSkyboxExportAsync(SkyboxExportRequest exportRequest, CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl($"skybox/export/{exportRequest.Id}"), parameters: new RestParameters(client.DefaultRequestHeaders), cancellationToken);
            response.Validate(EnableDebug);
            var skyboxOp = JsonConvert.DeserializeObject<SkyboxOperation>(response.Body, BlockadeLabsClient.JsonSerializationOptions);

            if (skyboxOp is not { Success: "true" })
            {
                throw new Exception($"Failed to cancel export for request {exportRequest.Id}!\n{skyboxOp?.Error}");
            }

            return skyboxOp.Success.Equals("true");
        }
    }
}
