
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

function createEvt(evt, kd) {
	return $('<tr>').append(
		$('<td>').append(
			$('<a>').attr({ 'href': '/event/' + evt.event.id }).text(evt.event.title)
		)
	).append(
		$('<td>').text(evt.next_time)
	).append(
		$('<td>').text(kd.name)
	);
}

$(document).ready(function () {
	$.get('/api/kids', function (data) {
		$('.loading').remove();
		
		var events_attended = [];

		data.forEach(kid => {
			$('#kids-events').append(createKid(kid));

			if (kid.attending.length != 0) {
				kid.attending.forEach(function(att) {
					events_attended.push({ 'att': att, 'kid': kid });
				});
			}
		});

		events_attended.forEach(function(el) {
			$('#completedEventsList').append(createEvt(el.att, el.kid));
		});
	});
});

