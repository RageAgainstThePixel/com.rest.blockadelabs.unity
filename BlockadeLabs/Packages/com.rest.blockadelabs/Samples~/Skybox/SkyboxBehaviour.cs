// Licensed under the MIT License. See LICENSE in the project root for license information.

using BlockadeLabs.Skyboxes;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Extensions;

namespace BlockadeLabs.Samples.Skybox
{
    public class SkyboxBehaviour : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField promptInputField;

        [SerializeField]
        private TMP_Dropdown skyboxStyleDropdown;

        [SerializeField]
        private Button generateButton;

        [SerializeField]
        private Material skyboxMaterial;

        private BlockadeLabsClient api;

#if !UNITY_2022_1_OR_NEWER
        private System.Threading.CancellationTokenSource lifetimeCancellationTokenSource = new();
        private System.Threading.CancellationToken destroyCancellationToken => lifetimeCancellationTokenSource.Token;
#endif

        private IReadOnlyList<SkyboxStyle> skyboxOptions;

        private void OnValidate()
        {
            promptInputField.Validate();
            skyboxStyleDropdown.Validate();
            generateButton.Validate();
        }

        private void Awake()
        {
            OnValidate();

            try
            {
                api = new BlockadeLabsClient();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create {nameof(BlockadeLabsClient)}!\n{e}");
                enabled = false;
                return;
            }

            GetSkyboxStyles();
            generateButton.onClick.AddListener(GenerateSkybox);
            promptInputField.onSubmit.AddListener(GenerateSkybox);
            promptInputField.onValueChanged.AddListener(ValidateInput);
        }

#if !UNITY_2022_1_OR_NEWER
        private void OnDestroy()
        {
            lifetimeCancellationTokenSource.Cancel();
            lifetimeCancellationTokenSource.Dispose();
            lifetimeCancellationTokenSource = null;
        }
#endif

        private void ValidateInput(string input)
        {
            generateButton.interactable = !string.IsNullOrWhiteSpace(input);
        }

        private void GenerateSkybox() => GenerateSkybox(promptInputField.text);

        private async void GenerateSkybox(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                Debug.LogWarning("No prompt given!");
                return;
            }

            try
            {
                generateButton.interactable = false;
                promptInputField.interactable = false;
                var request = new SkyboxRequest(skyboxOptions[skyboxStyleDropdown.value], prompt);
                var skyboxInfo = await api.SkyboxEndpoint.GenerateSkyboxAsync(request, cancellationToken: destroyCancellationToken).ConfigureAwait(true);

                if (skyboxInfo.TryGetAsset<Texture2D>("equirectangular-png", out var texture))
                {
                    skyboxMaterial.mainTexture = texture;
                    Debug.Log($"Successfully created skybox: {skyboxInfo.Id}");
                }
                else
                {
                    Debug.LogError("Failed to load texture for generated skybox!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                generateButton.interactable = true;
                promptInputField.interactable = true;
            }
        }


        private async void GetSkyboxStyles()
        {
            try
            {
                skyboxOptions = await api.SkyboxEndpoint.GetSkyboxStyleFamiliesAsync(null, destroyCancellationToken).ConfigureAwait(true);
                var dropdownOptions = new List<TMP_Dropdown.OptionData>(skyboxOptions.Count);
                dropdownOptions.AddRange(skyboxOptions.Select(skyboxStyle => new TMP_Dropdown.OptionData(skyboxStyle.Name)));
                skyboxStyleDropdown.options = dropdownOptions;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
