
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
				).append(
					$('<div>').addClass('btn-group inline pull-right').append(
						$('<a>').attr({ 'data-toggle': 'modal', 'data-target': '#scheduleModal', 'data-id': event.id, 'href': '#' }).addClass('btn btn-success btn-sm').append(
							$('<i>').addClass('fa fa-refresh').attr('aria-hidden', 'true')
						).append(
							' Re-Schedule'
						)
					)
				)/* .append(
					$('<div>').addClass('btn-group inline pull-right').append(
						$('<a>').addClass('btn btn-danger btn-sm').append(
							$('<i>').addClass('fa fa-trash-o').attr('aria-hidden', 'true')
						).append(
							' Delete'
							)
					)
				) */
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

$(document).ready(function () {
	$.get('/api/own_events', function (data) {
		$('.loading').remove();
		
		data.forEach(event => {
			$('#kids-events').append(createEvent(event));
		});
	});
});

$('#scheduleModal').on('show.bs.modal', function (e) {
	var eventId = $(e.relatedTarget).data('id');
	$(e.currentTarget).find('input[name="id"]').val(eventId);
});