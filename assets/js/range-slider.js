var app = angular.module('angularjs-starter', []);

app.controller('MainCtrl', function ($scope) {
    $scope.search = { price_min: '', price_max: '', amount_min: 1000, amount_max: 5000 };
});

/* Range Slider
    Input with default values:
    -min=0      // Min slider value
    -max=100    // Max slider value
    -step=1     // Steps

    Output / Input model
    -value-min    // Default value @min
    -value-max    // Default value @max

    example:
    <slider-range min="0" max="100" step="5" value-min="scope.form.slider_value_min" value-max="scope.form.slider_value_max"></slider-range>
*/
app.directive('sliderRange', ['$document', function ($document) {

    // Move slider handle and range line
    var moveHandle = function (handle, elem, posX) {
        $(elem).find('.handle.' + handle).css("left", posX + '%');
    };
    var moveRange = function (elem, posMin, posMax) {
        $(elem).find('.range').css("left", posMin + '%');
        $(elem).find('.range').css("width", posMax - posMin + '%');
    };

    return {
        template: '<div class="slider horizontal">' +
                    '<div class="range"></div>' +
                    '<a class="handle min" ng-mousedown="mouseDownMin($event)"></a>' +
                    '<a class="handle max" ng-mousedown="mouseDownMax($event)"></a>' +
                  '</div>',
        replace: true,
        restrict: 'E',
        scope: {
            valueMin: "=",
            valueMax: "="
        },
        link: function postLink(scope, element, attrs) {
            // Initilization
            var dragging = false;
            var startPointXMin = 0;
            var startPointXMax = 0;
            var xPosMin = 0;
            var xPosMax = 0;
            var settings = {
                "min": (typeof (attrs.min) !== "undefined" ? parseInt(attrs.min, 10) : 0),
                "max": (typeof (attrs.max) !== "undefined" ? parseInt(attrs.max, 10) : 100),
                "step": (typeof (attrs.step) !== "undefined" ? parseInt(attrs.step, 10) : 1)
            };
            if (typeof (scope.valueMin) == "undefined" || scope.valueMin === '')
                scope.valueMin = settings.min;

            if (typeof (scope.valueMax) == "undefined" || scope.valueMax === '')
                scope.valueMax = settings.max;

            // Track changes only from the outside of the directive
            scope.$watch('valueMin', function () {
                if (dragging) return;
                xPosMin = (scope.valueMin - settings.min) / (settings.max - settings.min) * 100;
                if (xPosMin < 0) {
                    xPosMin = 0;
                } else if (xPosMin > 100) {
                    xPosMin = 100;
                }
                moveHandle("min", element, xPosMin);
                moveRange(element, xPosMin, xPosMax);
            });

            scope.$watch('valueMax', function () {
                if (dragging) return;
                xPosMax = (scope.valueMax - settings.min) / (settings.max - settings.min) * 100;
                if (xPosMax < 0) {
                    xPosMax = 0;
                } else if (xPosMax > 100) {
                    xPosMax = 100;
                }
                moveHandle("max", element, xPosMax);
                moveRange(element, xPosMin, xPosMax);
            });

            // Real action control is here
            scope.mouseDownMin = function ($event) {
                dragging = true;
                startPointXMin = $event.pageX;

                // Bind to full document, to make move easiery (not to lose focus on y axis)
                $document.on('mousemove', function ($event) {
                    if (!dragging) return;

                    //Calculate handle position
                    var moveDelta = $event.pageX - startPointXMin;

                    xPosMin = xPosMin + ((moveDelta / element.outerWidth()) * 100);
                    if (xPosMin < 0) {
                        xPosMin = 0;
                    } else if (xPosMin > xPosMax) {
                        xPosMin = xPosMax;
                    } else {
                        // Prevent generating "lag" if moving outside window
                        startPointXMin = $event.pageX;
                    }
                    scope.valueMin = Math.round((((settings.max - settings.min) * (xPosMin / 100)) + settings.min) / settings.step) * settings.step;
                    scope.$apply();

                    // Move the Handle
                    moveHandle("min", element, xPosMin);
                    moveRange(element, xPosMin, xPosMax);
                });
                $document.mouseup(function () {
                    dragging = false;
                    $document.unbind('mousemove');
                    $document.unbind('mousemove');
                });
            };

            scope.mouseDownMax = function ($event) {
                dragging = true;
                startPointXMax = $event.pageX;

                // Bind to full document, to make move easiery (not to lose focus on y axis)
                $document.on('mousemove', function ($event) {
                    if (!dragging) return;

                    //Calculate handle position
                    var moveDelta = $event.pageX - startPointXMax;

                    xPosMax = xPosMax + ((moveDelta / element.outerWidth()) * 100);
                    if (xPosMax > 100) {
                        xPosMax = 100;
                    } else if (xPosMax < xPosMin) {
                        xPosMax = xPosMin;
                    } else {
                        // Prevent generating "lag" if moving outside window
                        startPointXMax = $event.pageX;
                    }
                    scope.valueMax = Math.round((((settings.max - settings.min) * (xPosMax / 100)) + settings.min) / settings.step) * settings.step;
                    scope.$apply();

                    // Move the Handle
                    moveHandle("max", element, xPosMax);
                    moveRange(element, xPosMin, xPosMax);
                });

                $document.mouseup(function () {
                    dragging = false;
                    $document.unbind('mousemove');
                    $document.unbind('mousemove');
                });
            };
        }
    };
}]);