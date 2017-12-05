var slider = "#price-max";
var slider_output = "#price-slider-value";

function update_slider_value() {
	$(slider_output).html($(slider).val()).append('&nbsp;<span class="fa fa-money"></span>');
}

$(slider).change(update_slider_value);

// Modal Interchange
function spawnModal(modal) {
	if (modal === 'signup') {
		$('#loginModal').modal('hide');
		$('#signupModal').modal('show');
	}

	if (modal === 'login') {
		$('#signupModal').modal('hide');
		$('#loginModal').modal('show');
	}
}

// TODO: wrap the following functions properly
$('#loginForm').submit(function (event) {
	event.preventDefault();

	var inp = $('#loginForm').serializeArray();

	$.post('/login',
		{
			email: inp[0].value,
			password: inp[1].value
		},
		function (resp, status) {
			$('#loginModal').modal('hide');
			location.href = "/events";
		});
});

function registerLoginSignupHandlers(selector) {
	$(selector).submit(function(event) {
		event.preventDefault();

		var inp = $(selector).serializeArray();

		if (inp[5].value != "on") {
			alert('You need to accept the Terms of Usage in order to sign up.');
			return;
		}

		$('.submitAwait').removeClass('fa fa-sign-in').addClass('fa fa-spinner fa-spin fa-fw');

		$.post('/register',
			{
				email: inp[0].value,
				password: inp[3].value,
				password2: inp[4].value,
				full_name: inp[1].value + ' ' + inp[2].value,
				role: $('signupParentPill').hasClass('active') ? 1 : 2
			},
			function (resp, status) {
				// TODO: check status, handle errors (maybe use classes 'alert alert-dismissible alert-success' to display messages)
				$('#signupModal').modal('hide');
				location.href = "/events";
			});
	});
}

registerLoginSignupHandlers('#signupParentForm');
registerLoginSignupHandlers('#signupOrganizerForm');

