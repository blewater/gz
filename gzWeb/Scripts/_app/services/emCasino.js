(function() {
    "use strict";

    APP.factory("emCasino", ["emWamp", emCasinoFunc]);
    function emCasinoFunc(emWamp) {

        var _service = {};

        _service.FIELDS = {
            Slug: 1,
            Vendor: 2,
            Name: 4,
            ShortName: 8,
            Description: 16,
            AnonymousFunMode: 32,
            FunMode: 64,
            RealMode: 128,
            NewGame: 256,
            License: 512,
            Popularity: 1024,
            Width: 2048,
            Height: 4096,
            Thumbnail: 8192,
            Logo: 16384,
            BackgroundImage: 32768,
            Url: 65536,
            HelpUrl: 131072,
            Categories: 262144,
            Tags: 524288,
            Platforms: 1048576,
            RestrictedTerritories: 2097152,
            TheoreticalPayOut: 4194304,
            BonusContribution: 8388608,
            JackpotContribution: 16777216,
            FPP: 33554432,
            Limitation: 536870912,
            Currencies: 8589934592,
            Languages: 17179869184
        };

        _service.getGameCategories = function () {
            return emWamp.call("/casino#getGameCategories");
        };

        /// <summary>
        /// Query the casino games according to specific condition indicated in input parameters.
        /// </summary>
        /// <parameter name="filterByName">
        /// [string array, optional]
        /// Filter the games against game name. Only the games whose name exists in this array are returned.
        /// </parameter>
        /// <parameter name="filterBySlug">
        /// [string array, optional]
        /// Filter the games against game slug, the unique identity of the game. Only the games whose slug exists in this array are returned.
        /// </parameter>
        /// <parameter name="filterByVendor">
        /// [string array, optional]
        /// Filter the games against vendors. Only the games whose vendor exists in this array are returned.
        /// </parameter>
        /// <parameter name="filterByCategory">
        /// [string array, optional]
        /// Filter the games against categories. Only the games whose category exists in this array are returned.
        /// </parameter>
        /// <parameter name="filterByTag">
        /// [string array, optional]
        /// Filter the games against tags. Only the games whose tag exists in this array are returned
        /// </parameter>
        /// <parameter name="filterByAttribute">
        /// [JSON object]
        /// Filter the games against attributes. Only the below attributes are supported:
        /// <code>
        /// {
        ///     "newGame": true //true or false, only new / non-new games are returned
        /// }
        /// </code>
        /// </parameter>
        /// <parameter name="expectedFields">
        /// [integer, mandatory]
        /// To reduce the size of the returned data for the method, this parameter is used to indicate the game fields expected in JSON response. 
        /// Individual field value is defined as below. Passing this field with the sum of the expected fields' value.
        /// </parameter>
        //_service.getGames = function (filterByName, filterBySlug, filterByVendor, filterByCategory, filterByTag, filterByAttribute, expectedFields, pageIndex, pageSize) {
        //    //angular.forEach(ex)
        //    return emWamp.call("/casino#getGames",
        //    {
        //        filterByName: filterByName,
        //        filterBySlug: filterBySlug,
        //        filterByVendor: filterByVendor,
        //        filterByCategory: filterByCategory,
        //        filterByTag: filterByTag,
        //        filterByAttribute: filterByAttribute,
        //        expectedFields: expectedFields,
        //        expectedFormat: "array",
        //        pageIndex: pageIndex,
        //        pageSize: pageSize,
        //    });
        //};

        _service.getGameCategories = function () {
            return emWamp.call("/casino#getGameCategories");
        };

        _service.getGames = function (parameters) {
            return emWamp.call("/casino#getGames", parameters);
        };

        _service.getGameVendors = function () {
            return emWamp.call("/casino#getGameVendors");
        };

        return _service;
    };

})();