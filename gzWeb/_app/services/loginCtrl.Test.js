/// <reference path="../../jquery/jquery-1.12.0.js" />
/// <reference path="../../autobahn.min.js" />

/// <reference path="../../angular/angular.js" />
/// <reference path="../../angular/angular-mocks.js" />
/// <reference path="../../angular-autocomplete/ngAutocomplete.js" />
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
/// <reference path="../../angular-recaptcha/angular-recaptcha.min.js" />
/// <reference path="../../angular-wamp/angular-wamp.js" />
/// <reference path="../../angular-fullscreen/angular-fullscreen.js" />
/// <reference path="../app.js" />
/// <reference path="../configuration/config.js" />
/// <reference path="../constants/constants.js" />
/// <reference path="../../_modules/customDirectives.js" />
/// <reference path="../../_modules/customFilters.js" />
/// <reference path="apiService.js" />
/// <reference path="nsMessageService/nsMessageService.js" />
/// <reference path="routeService.js" />
/// <reference path="authInterceptor.js" />
/// <reference path="../controllers/auth/loginCtrl.js" />
/// <reference path="emWamp.js" />

describe("loginCtrl", function () {

    var $rootScope, $controller, emWamp, api, localStorageService, constants, $q, $httpBackend;

    beforeEach(module("GZ"));

    //beforeEach(module(function($provide, $q) {
    //    $provide.service('emWamp', function () {
            
    //            this.login = jasmine.createSpy("login").and.callFake(function(params) {
    //                var deferred = $q.defer();
    //                if (params.usernameOrEmail === "gz2016" && params.password === "gz2016!@") {
    //                    deferred.resolve({
    //                        hasToAcceptTC: false,
    //                        minorChangeInTC: false,
    //                        majorChangeInTC: false,
    //                        isEmailVerified: true,
    //                        isProfileIncomplete: false,
    //                        roles: [],
    //                        hasToEnterCaptcha: false,
    //                        hasToChangePassword: true,
    //                        loginCount: 0,
    //                        registrationTime: "",
    //                        lastLoginTime: ""
    //                    });
    //                }
    //                return deferred.promise;
    //            });
    //    });
    //}));

    beforeEach(inject(function (_$rootScope_, _$controller_, _api_, _localStorageService_, _constants_, _emWamp_, _$q_, $injector) {
        $rootScope = _$rootScope_;
        $controller = _$controller_;
        api = _api_;
        localStorageService = _localStorageService_;
        constants = _constants_;
        emWamp = _emWamp_;
        $q = _$q_;

        $httpBackend = $injector.get('$httpBackend');
        $httpBackend.whenGET('/Mvc/Guest/Home').respond();
    }));

    describe("$scope.model", function () {

        var $scope, controller;

        beforeEach(function() {
            $scope = $rootScope.$new();
            $scope.form = {};
            controller = $controller('loginCtrl', { $rootScope: $rootScope, $scope: $scope, emWamp: emWamp, api: api, localStorageService: localStorageService, constants: constants });
        });

        it("should not fail when login", function (done) {

            // Fakes
            $scope.form.$valid = true;
            $scope.nsOk = function() {};

            $scope.usernameOrEmail = "gz2016";
            $scope.password = "gz2016!@";
            
            expect($scope.waiting).toBe(false);

            var emDeferred = $q.defer();
            emDeferred.resolve({
                hasToAcceptTC: false,
                minorChangeInTC: false,
                majorChangeInTC: false,
                isEmailVerified: true,
                isProfileIncomplete: false,
                roles: [],
                hasToEnterCaptcha: false,
                hasToChangePassword: true,
                loginCount: 0,
                registrationTime: "",
                lastLoginTime: ""
            });

            var gzDeferred = $q.defer();
            gzDeferred.resolve({ data: { access_token: 'access_token', userName: 'userName' } });

            spyOn(emWamp, "login").and.returnValue(emDeferred.promise);
            spyOn(api, "login").and.returnValue(gzDeferred.promise);
            //$httpBackend.whenPOST('/TOKEN').respond({ data: { access_token: 'access_token', userName: 'userName' } });

            $scope.submit();
            
            $scope.$apply();

            expect($scope.waiting).toBe(false);
            expect($scope.errorMsg).toBe("");
            
            done();
        });
    });
});