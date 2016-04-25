(function () {
    'use strict';
    var ctrlId = 'registerStartCtrl';
    APP.controller(ctrlId, ["$scope", "$http", "$filter", "emWamp", "api", "constants", ctrlFactory]);
    function ctrlFactory($scope, $http, $filter, emWamp, api, constants) {
        $scope.model = {
            email: null,
            username: null,
            password: null
        };

        // #region Email
        $scope.emailValidation = {
            isValidating: false,
            isAvailable: true,
            error: ''
        };
        $scope.resetEmailValidation = function () {
            $scope.emailValidation.isValidating = false;
            $scope.emailValidation.isAvailable = true;
            $scope.emailValidation.error = '';
        };
        $scope.validateEmail = function (email) {
            if (!$scope.emailValidation.isValidating) {
                $scope.emailValidation.isValidating = true;
                return emWamp.call('/user/account#validateEmail', { email: email })
                    .then(function (result) {
                        $scope.emailValidation.isValidating = false;
                        $scope.emailValidation.isAvailable = result.isAvailable;
                        $scope.emailValidation.error = result.error;
                    }, function (error) {
                        $scope.emailValidation.isValidating = false;
                        logError(error);
                    });
            }
        };
        // #endregion

        // #region Username
        $scope.usernameValidation = {
            isValidating: false,
            isAvailable: true,
            error: ''
        };
        $scope.resetUsernameValidation = function () {
            $scope.usernameValidation.isValidating = false;
            $scope.usernameValidation.isAvailable = true;
            $scope.usernameValidation.error = '';
        };
        $scope.validateUsername = function (username) {
            if (!$scope.usernameValidation.isValidating) {
                return emWamp.call('/user/account#validateUsername', { username: username })
                    .then(function (result) {
                        $scope.usernameValidation.isValidating = false;
                        $scope.usernameValidation.isAvailable = result.isAvailable;
                        $scope.usernameValidation.error = result.error;
                    }, function (error) {
                        $scope.usernameValidation.isValidating = false;
                        logError(error);
                    });
            }
        };
        // #endregion

        // #region Password
        var _passwordPolicyRegEx = '';
        var _passwordPolicyError = '';

        function getPasswordPolicy() {
            emWamp.call('/user/pwd#getPolicy')
                .then(function (result) {
                    _passwordPolicyRegEx = new RegExp(result.regularExpression);
                    _passwordPolicyError = result.message;
                }, logError);
        };

        $scope.passwordValidation = {
            //isValidating: false,
            isValid: true,
            error: ''
        };
        $scope.resetPasswordValidation = function() {
            $scope.passwordValidation.isValid = true;
            $scope.passwordValidation.error = '';
        };
        $scope.validatePassword = function (password) {
            if (_passwordPolicyRegEx.test(password)) {
                $scope.passwordValidation.isValid = true;
            } else {
                $scope.passwordValidation.isValid = false;
                $scope.passwordValidation.error = _passwordPolicyError;
            };
        };
        // #endregion


        $scope.backToLogin = function () {
            $scope.nsBack({
                nsType: 'modal',
                nsSize: 'sm',
                nsTemplate: '/partials/messages/login.html',
                nsCtrl: 'loginCtrl',
                nsStatic: true,
            });
        };
        $scope.submit = function () {
            if ($scope.form.$valid)
                proccedToUserDetails();
        };
        function proccedToUserDetails(){
            $scope.nsNext({
                nsType: 'modal',
                nsSize: 'md',
                nsTemplate: '/partials/messages/registerDetails.html',
                nsCtrl: 'registerDetailsCtrl',
                nsStatic: true,
                nsParams: { startModel: $scope.model }
            });
        }

        //$scope.register = function () {
        //    var emRegisterQ = emRegister("(empty callbackUrl)");
        //    emRegisterQ.then(function(result) {
        //        emWamp.login({
        //            usernameOrEmail: $scope.model.username,
        //            password: $scope.model.password
        //        })
        //        .then(function(result) {
        //            gzRegister().then(function(result) {
        //                //api.login($scope.model.username, $scope.model.password)
        //                //    .then(function(result) {
        //                //        // TODO: inform user...
        //                //    }, logError);
        //            }, logError);
        //        }, logError);
        //    }, logError);
        //};

        //function emRegister(callbackUrl) {
        //    return emWamp.register({
        //        username: $scope.model.username,
        //        email: $scope.model.email,
        //        alias: $scope.model.username,
        //        password: $scope.model.password,
        //        firstname: $scope.model.firstname,
        //        surname: $scope.model.lastname,
        //        birthDate: moment([$scope.model.yearOfBirth, $scope.model.monthOfBirth - 1, $scope.model.dayOfBirth]).format('YYYY-MM-DD'),
        //        country: $scope.model.country.code,
        //        // TOOD: region 
        //        // TOOD: personalID 
        //        mobilePrefix: $scope.model.phonePrefix,
        //        mobile: $scope.model.phoneNumber,
        //        currency: $scope.model.currency.code,
        //        title: $scope.model.title,
        //        gender: "M",
        //        city: $scope.model.city,
        //        address1: $scope.model.address,
        //        address2: '',
        //        postalCode: $scope.model.postalCode,
        //        language: 'en',
        //        emailVerificationURL: callbackUrl,
        //        securityQuestion: "(empty security question)",
        //        securityAnswer: "(empty security answer)"
        //    });
        //}

        //function gzRegister() {
        //    return $http.post('/api/Account/Register', {
        //        Username: $scope.model.username,
        //        Email: $scope.model.email,
        //        Password: $scope.model.password,
        //        FirstName: $scope.model.firstname,
        //        LastName: $scope.model.lastname,
        //        Birthday: moment([$scope.model.yearOfBirth, $scope.model.monthOfBirth - 1, $scope.model.dayOfBirth]),
        //        Currency: $scope.model.currency.code,
        //        Title: $scope.model.title,
        //        Country: $scope.model.country.code,
        //        MobilePrefix: $scope.model.phonePrefix,
        //        Mobile: $scope.model.phoneNumber,
        //        City: $scope.model.city,
        //        Address: $scope.model.address,
        //        PostalCode: $scope.model.postalCode,
        //        // TODO: Region ???
        //    });
        //}


        function logError(error) {
            console.log(error);
        };


        function init() {
            getPasswordPolicy();
        };
        init();
    }
})();