
// <copyright company="Softerra">
//    Copyright (c) Softerra, Ltd. All rights reserved.
// </copyright>
//
// <summary>
//    Contains key codes.
// </summary>

(function (angular)
{

    /**
       * 
       * @name keyCode
       * @desc Contains key codes.
       * 
       */
    angular.module('takinApp.common').constant('keyCode', {
        BACKSPACE: 8,
        TAB: 9,
        ENTER: 13,
        SHIFT: 16,
        ESCAPE: 27,
        SPACE: 32,
        PAGE_UP: 33,
        PAGE_DOWN: 34,
        LEFT: 37,
        UP: 38,
        RIGHT: 39,
        DOWN: 40,
        
        // which code 
        DASH: 45,
        DELETE: 46,
        NUM_0: 48,
        NUM_9: 57,

        CH_A: 65
    });

})(angular);