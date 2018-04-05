/// <reference path="apiService.js" />
/// <reference path="authInterceptor.js" />

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