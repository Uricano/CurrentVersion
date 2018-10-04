app.directive("toolTip", function () {
    return {
        restrict: "A",
        replace: true,
        scope: {
            tpText:'='
        },
        link: function (scope, element, attrs, ngModelCtrl) {
            var target = false,
            tooltip = false,
            title = false;
            var text = scope.tpText ? scope.tpText : attrs.title;

            element.on('mouseenter', function () {
                target = $(this);
                tip = text;//attrs.title;
                tooltip = $('<div id="tooltip"></div>');

                if (!tip || tip == '')
                    return false;

                target.removeAttr('title');
                tooltip.css('opacity', 0)
                       .html(tip)
                       .appendTo('body');

                var init_tooltip = function () {
                    if ($(window).width() < tooltip.outerWidth() * 1.5)
                        tooltip.css('max-width', $(window).width() / 2);
                    else
                        tooltip.css('max-width', 340);

                    var pos_left = target.offset().left + (target.outerWidth() / 2) - (tooltip.outerWidth() / 2),
                        pos_top = target.offset().top - tooltip.outerHeight() - 20;

                    if (pos_left < 0) {
                        pos_left = target.offset().left + target.outerWidth() / 2 - 20;
                        tooltip.addClass('left');
                    }
                    else
                        tooltip.removeClass('left');

                    if (pos_left + tooltip.outerWidth() > $(window).width()) {
                        pos_left = target.offset().left - tooltip.outerWidth() + target.outerWidth() / 2 + 20;
                        tooltip.addClass('right');
                    }
                    else
                        tooltip.removeClass('right');

                    if (pos_top < 0) {
                        var pos_top = target.offset().top + target.outerHeight();
                        tooltip.addClass('top');
                    }
                    else
                        tooltip.removeClass('top');

                    tooltip.css({ left: pos_left, top: pos_top })
                           .animate({ top: '+=10', opacity: 1 }, 50);
                };

                setTimeout(function () {
                    init_tooltip();
                    $(window).resize(init_tooltip);
                }, 600);

                var remove_tooltip = function () {
                    tooltip.animate({ top: '-=10', opacity: 0 }, 50, function () {
                        $(this).remove();
                    });

                    target.attr('title', tip);
                };

                target.bind('mouseleave', remove_tooltip);
                tooltip.bind('click', remove_tooltip);
                setTimeout(function () { remove_tooltip(); }, 6000);
            });

            scope.$on('initTooltipText', function (event,txt) {

                scope.tpText = txt ? txt : scope.tpText;

                 text = scope.tpText ? scope.tpText : attrs.title;

            })
        }

    }
});