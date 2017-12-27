
function createKid(name, gender, age) {
	return $('<div>').addClass('panel-body').append(
		$('<div>').addClass('media').append(
			$('<div>').addClass('media-left').append(
				$('<img>').attr('src', 'img_avatar1.png').addClass('media-object').attr('style', 'width:60px;')
			)
		).append(
			$('<div>').addClass('media-body').append(
				$('<h4>').addClass('media-heading').append(name)
			).append(
				$('<p>').append('Age: ' + age)
			).append(
			$('<p>').append('Gender: ' + (gender === 0 ? 'Male' : 'Female'))
			)
		)
	)
}

$(document).ready(function () {
	$.get( '/api/kids', function (data) {
			data.forEach(kid => {
				$('#users_kids').append(createKid(kid.name, kid.gender, kid.age));
			});
		}
	);
});

