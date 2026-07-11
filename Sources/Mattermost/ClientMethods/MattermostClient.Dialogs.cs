using Mattermost.Constants;
using Mattermost.Models.Dialogs;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mattermost
{
    public partial class MattermostClient
    {
        /// <summary>
        /// Open an interactive dialog.
        /// </summary>
        /// <param name="triggerId"> Trigger identifier from a slash command or interactive action payload. </param>
        /// <param name="url"> URL where Mattermost sends the submitted dialog payload. </param>
        /// <param name="dialog"> Dialog definition. </param>
        public Task OpenInteractiveDialogAsync(string triggerId, string url, InteractiveDialog dialog)
        {
            OpenInteractiveDialogRequest request = new OpenInteractiveDialogRequest
            {
                TriggerId = triggerId,
                Url = url,
                Dialog = dialog
            };
            return OpenInteractiveDialogAsync(request);
        }

        /// <summary>
        /// Open an interactive dialog.
        /// </summary>
        /// <param name="request"> Open dialog request. </param>
        public Task OpenInteractiveDialogAsync(OpenInteractiveDialogRequest request)
        {
            CheckDisposed();
            ValidateOpenInteractiveDialogRequest(request);
            return SendRequestAsync(HttpMethod.Post, Routes.Dialogs + "/open", request);
        }

        private static void ValidateOpenInteractiveDialogRequest(OpenInteractiveDialogRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ThrowIfWhiteSpace(request.TriggerId, nameof(request.TriggerId));
            ThrowIfWhiteSpace(request.Url, nameof(request.Url));
            ValidateInteractiveDialog(request.Dialog);
        }

        private static void ValidateInteractiveDialog(InteractiveDialog dialog)
        {
            if (dialog == null)
            {
                throw new ArgumentException("Dialog cannot be null.", nameof(dialog));
            }

            ThrowIfWhiteSpace(dialog.Title, nameof(dialog.Title));
            if (dialog.Elements == null)
            {
                throw new ArgumentException("Dialog elements cannot be null.", nameof(dialog.Elements));
            }

            foreach (InteractiveDialogElement element in dialog.Elements)
            {
                ValidateInteractiveDialogElement(element);
            }
        }

        private static void ValidateInteractiveDialogElement(InteractiveDialogElement element)
        {
            if (element == null)
            {
                throw new ArgumentException("Dialog element cannot be null.", nameof(element));
            }

            ThrowIfWhiteSpace(element.DisplayName, nameof(element.DisplayName));
            ThrowIfWhiteSpace(element.Name, nameof(element.Name));
            if (!element.Type.HasValue)
            {
                throw new ArgumentException("Dialog element type must be specified.", nameof(element.Type));
            }
        }

        private static void ThrowIfWhiteSpace(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(parameterName + " cannot be null or empty.", parameterName);
            }
        }
    }
}
