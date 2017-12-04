var slider = "#price-max";
var slider_output = "#price-slider-value";

function update_slider_value() {
	$(slider_output).html($(slider).val()).append('&nbsp;<span class="fa fa-money"></span>');
}

$(slider).change(update_slider_value);

// Modal Interchange
function spawnModal(modal) {
	if (modal === 'signup')
	{
		$('#loginModal').modal('hide');
		$('#signupModal').modal('show');
	}
	
	if (modal === 'login')
	{
		$('#signupModal').modal('hide');
		$('#loginModal').modal('show');
	}
}
