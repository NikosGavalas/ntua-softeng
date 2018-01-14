
function createEvent(event) {
	return $('<div>').addClass('panel-body').append(
		$('<div>').addClass('media').append(
			$('<div>').addClass('media-left').append(
				$('<img>').attr('src', event.thumbnail).addClass('media-object').attr('style', 'width:60px;')
			)
		).append(
			$('<div>').addClass('media-body').append(
				$('<h4>').addClass('media-heading').append(
					$('<a>').attr('href', '/event/' + event.id).append(event.title)
				)
			).append(
				$('<p>').append('Duration: ' + event.duration)
			).append(
				$('<p>').append('Price: ' + event.price)
			).append(
				$('<p>').append('Address: ' + event.address)
			).append(
				$('<p>').append('Gender: ' + event.gender)
			).append(
				$('<p>').append('Next Occurence: ' + event.scheduled[0].next_time)
			)
		)
	)
}

$(document).ready(function () {
	$.get('/api/own_events', function (data) {
		data.forEach(event => {
			$('#users_kids').append(createEvent(event));
		});
	}
	);
});

