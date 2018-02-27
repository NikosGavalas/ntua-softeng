
function createKid(kid) {
	return $('<div>').addClass('panel-body').append(
		$('<div>').addClass('media').append(
			$('<div>').addClass('media-left').append(
				$('<img>').attr('src', kid.avatar).addClass('media-object').attr('style', 'width:60px;')
			)
		).append(
			$('<div>').addClass('media-body').append(
				$('<h4>').addClass('media-heading').append(kid.name)
			).append(
				$('<p>').append('Age: ' + kid.age)
			).append(
				$('<p>').append('Gender: ' + (kid.gender === 0 ? 'Male' : 'Female'))
			)
		)
	)
}

$(document).ready(function () {
	$.get('/api/kids', function (data) {
		$('.loading').remove();
			
		data.forEach(kid => {
			console.log(kid);
			$('#kids-events').append(createKid(kid));
		});
	});
});

