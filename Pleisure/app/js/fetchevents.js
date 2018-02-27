
function createEvent(event) {
	var callstring = 'rescheduleEvent(' + event.id + ');'

	return $('<div>').addClass('panel-body').append(
		$('<div>').addClass('media').append(
			$('<div>').addClass('media-left').append(
				$('<img>').attr('src', event.thumbnail).addClass('media-object').attr('style', 'width:60px;')
			)
		).append(
			$('<div>').addClass('media-body').append(
				$('<h4>').addClass('media-heading').append(
					$('<a>').attr('href', '/event/' + event.id).append(event.title)
				).append(
					$('<div>').addClass('btn-group inline pull-right').append(
						$('<a>').attr({ 'onclick': callstring, 'data-id': event.id, 'href': '#' }).addClass('btn btn-success btn-sm').append(
							$('<i>').addClass('fa fa-refresh').attr('aria-hidden', 'true')
						).append(
							' Re-Schedule'
						)
					).append(
						$('<div>').addClass('btn-group inline pull-right').append(
							$('<a>').addClass('btn btn-success btn-sm')
								.append(
									' Attendees'
								)
						)
					)
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
	).append(
		$('<hr>')
	)
}

function createCompletedEvent(event) {
	return $('<tr>').append(
		$('<td>').append(
			$('<a>').attr({ 'href': '/event/' + event.id }).text(event.title)
		)
	).append(
		$('<td>').text(event.scheduled[0].next_time)
	)
}

$(document).ready(function () {
	$.get('/api/own_events', function (data) {
		$('.loading').remove();
		
		data.forEach(event => {

			// I AM SERIOUSLY NOT PROUD OF THIS

			event.scheduled = event.scheduled.sort((a, b) => {
				var date1 = a.next_time;
				var date2 = b.next_time;

				date1 = swapDate(date1);				
				date2 = swapDate(date2);		

				return date1 > date2;
			});

			var d = new Date();

			var event_date = swapDate(event.scheduled[0].next_time);
			var this_month = d.getMonth() + 1;
			var now = [d.getFullYear(), this_month < 10 ? '0' + this_month : this_month, d.getDate()].join('/');

			if (event_date > now)
				$('#kids-events').append(createEvent(event));
			else {
				$('#completedEventsList').append(createCompletedEvent(event));
			}
		});
	});
});

$('#scheduleModal').on('show.bs.modal', function (e) {
	var eventId = $(e.relatedTarget).data('id');
	$(e.currentTarget).find('input[name="id"]').val(eventId);
});

function swapDate(date) {
	var ret = date.split(' ');
	ret = ret[0].split('/');

	var temp = ret[0];
	ret[0] = ret[2];
	ret[2] = temp;

	return ret.join('/');
}