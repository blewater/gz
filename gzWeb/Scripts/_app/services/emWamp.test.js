/// <reference path="../../jquery/jquery-1.12.0.js" />
/// <reference path="../../autobahn.min.js" />

/// <reference path="../../angular/angular.js" />
/// <reference path="../../angular/angular-mocks.js" />
/// <reference path="../../angular-route/angular-route.js" />
/// <reference path="../../angular-resource/angular-resource.js" />
/// <reference path="../../angular-sanitize/angular-sanitize.js" />
/// <reference path="../../angular-animate/angular-animate.js" />
/// <reference path="../../angular-cookies/angular-cookies.js" />
/// <reference path="../../angular-filter/angular-filter.js" />
/// <reference path="../../angular-moment/angular-moment.js" />
/// <reference path="../../angular-spinner/angular-spinner.js" />
/// <reference path="../../angular-ui/ui-bootstrap.js" />
/// <reference path="../../angular-match-media/match-media.js" />
/// <reference path="../../angular-local-storage/angular-local-storage.js" />
/// <reference path="../../angular-count-to/angular-count-to.min.js" />
/// <reference path="../../angular-wamp/angular-wamp.js" />
/// <reference path="../../angular-fullscreen/angular-fullscreen.js" />
/// <reference path="../app.js" />
/// <reference path="../configuration/config.js" />
/// <reference path="../constants/constants.js" />
/// <reference path="../../_modules/customDirectives.js" />
/// <reference path="../../_modules/customFilters.js" />
/// <reference path="routeService.js" />
/// <reference path="authInterceptor.js" />
/// <reference path="emWamp.js" />

//https://autobahn.s3.amazonaws.com/autobahnjs/latest/autobahn.min.jgz

describe("emWamp service", function () {
    var emWamp;
    var originalTimeout;
    
    //beforeAll(function (done) {
    //    module("GZ");
    //    inject(function($injector) {
    //        emWamp = $injector.get("emWamp");
    //    });
    //    done();
    //});

    beforeEach(function() {
        module("GZ");

        var i = angular.injector(["ng"]),
        rs = i.get("$rootScope");

        flush = function () {
            rs.$apply();
        }

        module("GZ", function ($provide) {
            $provide.value("$rootScope", rs);
        });
    });

    beforeEach(function() {
        inject(function(_emWamp_) {
            emWamp = _emWamp_;
        });
        originalTimeout = jasmine.DEFAULT_TIMEOUT_INTERVAL;
        jasmine.DEFAULT_TIMEOUT_INTERVAL = 30000;
        flush();
    });

    afterEach(function () {
        jasmine.DEFAULT_TIMEOUT_INTERVAL = originalTimeout;
    });

    //beforeEach(function (done) {
    //    module("GZ");
    //    inject(function ($injector) {
    //        emWamp = $injector.get("emWamp");
    //    });
    //    done();
    //});

    it("should work", function (done) {
        emWamp.userLogin({ usernameOrEmail: 'xdinos1@nessos.gr', password: 'lunat!c7' })
            .then(function (result) {
                console.log(result);
                expect(true).toBe(true);
                done();
            }, function() {
                expect(false).toBe(true);
                done();
            });
        flush();
    });

    it("should work 2", function (done) {
        expect(true).toBe(true);
        done();
        //emWamp.userGetSessionInfo()
        //    .then(function (result) {
        //        expect(true).toBe(true);
        //        done();
        //    }, function () {
        //        expect(false).toBe(true);
        //        done();
        //    });
        //flush();
    });
})