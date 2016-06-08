(function () {
    'use strict';
    var ctrlId = 'contactCtrl';
    APP.controller(ctrlId, ['$scope', ctrlFactory]);
    function ctrlFactory($scope) {
    }

    function initialize() {
        var myLatLng = { lat: 51.5196508, lng: -0.1007646 };

        var mapProp = new google.maps.Map(document.getElementById('gz-map'), {
            center: myLatLng,
            zoom: 16,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        });

        var marker = new google.maps.Marker({
            position: myLatLng,
            map: mapProp,
            title: 'Greenzorro LTD. 80-83 , Long Lane LONDON EC1A 9ET'
        });

    };

    google.maps.event.addDomListener(window, 'load', initialize);

})();