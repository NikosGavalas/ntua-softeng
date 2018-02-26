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

// Payment Modal: construct options for cc exp year field
var year = new Date().getFullYear();

for(var i = 0; i < 10; i++) {
	$('#expDateYear').append(
		$('<option>').text(year + i).attr('value', year + i)
	)
}

$('#signupForm').submit(function (event) {
	var inp = $('#signupForm').serializeArray();

	if (inp[4].value != "on") {
		alert('You need to accept the Terms of Usage in order to sign up.');
		event.preventDefault();
		return;
	}

	// TODO: also if (password fields are not the same)...
	if (!inp[0].value || !inp[1].value || !inp[2].value || !inp[3].value) {
		alert('Please fill all the fields.');
		event.preventDefault();
		return;
	}

	$('#signupRole').val($('#signupOrganizerPill').hasClass('active') ? 2 : 1);

	$('.submitAwait').removeClass('fa fa-sign-in').addClass('fa fa-spinner fa-spin fa-fw');

});


function editProfileModal(userId)
{
	$.get({
		url: '/api/user',
		data: {
			user_id: userId
		},
		success: function(user)
		{
			$('#editProfileEmailField').val(user.email);
			$('#editProfileFullNameField').val(user.fullname);
			$('#editProfileAddressField').val(user.address);

			$('#editProfileModal').modal();
		}
	});
}


$(document).ready(function () {
	$('.datetimepicker').datetimepicker();
});

$('.full-height-modal').on('show.bs.modal', function () {
	$('.modal-body').css('height', $(window).height() * 0.7);
	$('.modal .modal-body').css('overflow', 'auto');
});

function infoRequireLogin(params) {
	$('#requireLoginAlertBox').show();
}
