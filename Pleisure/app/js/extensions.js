$(document).ready(function () {
	/*
	args = {
		onSubmit: function(form) 
		{
			Called when the form is submitted
			If this returns false, the form will not be submitted
		},
		onResponse: function(response, status, xhr)
		{
			Called when the response is received
		},
		onError: function(xhr, status, error)
		{
			Called on error
		},
		onProgress: function(progressPercent)
		{
			Called on upload progress
		},
		onDownloadProgress: function(progressPercent)
		{
			Called on download progress
		},
		method: 'POST'    // the method to submit the form with, default is POST
	}
	*/
	$.fn.ajaxForm = function (args) {
		this.unbind().submit(
			function (event) {
				var thisForm = $(this);
				var btnContainer = $(this).find('.ajax-btn-container');
				var loadingContainer = $(this).find('.ajax-loading');

				event.preventDefault();

				btnContainer.hide();
				loadingContainer.show();

				if (args.onSubmit && !args.onSubmit(thisForm))
				{
					return;
				}

				thisForm.ajaxSubmit({
					type: args.method ? args.method : 'POST',
					success: function (resp, status, xhr) {
						loadingContainer.hide();
						btnContainer.show();

						if (args.onResponse)
						{
							args.onResponse(resp, status, xhr);
						}
					},
					error: function (xhr, status, error) {
						if (args.onError)
						{
							args.onError(xhr, status, error);
						}
					},
					xhr: function () {
						var xhr = new window.XMLHttpRequest();
						xhr.upload.addEventListener('progress', function (evt) {
							if (evt.lengthComputable) {
								var percent = evt.loaded / evt.total;

								if (args.onProgress)
								{
									args.onProgress(percent);
								}
							}
						}, false);
						xhr.addEventListener('progress', function (evt) {
							if (evt.lengthComputable) {
								var percent = evt.loaded / evt.total;

								if (args.onDownloadProgress)
								{
									args.onDownloadProgress(percent);
								}
							}
						}, false);

						return xhr;
					}
				});
			}
		);
		return this;
	};

	/*
		To be used on inputs like files, checkboxes, radios etc. so that their form is submitted when they are set.
	*/
	$.fn.ajaxBtn = function () {
		this.change(
			function (e) {
				$(this).closest('form').submit();
			}
		);
	}
});