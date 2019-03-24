if (!isTouchDevice()) {
  $('[data-toggle*="tooltip"]').tooltip();
}

// utility

function isTouchDevice() {
  return !!('ontouchstart' in window || navigator.msMaxTouchPoints);
}


$(document).ready(function () {
  // Register sliders
  $('[data-role="rangeslider"] > input').change(function (evt) {
    // Update the label above the slider
    $(this).parent()
      .find('.slider-value')
      .html($(this).val());
  });

  // Trigger a change on each for the initial update
  $('[data-role="rangeslider"] > input').change();
});