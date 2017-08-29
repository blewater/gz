(function() {
    "use strict";

    APP.factory("emCasino", ['emWamp', emCasinoFunc]);
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
        _service.getGames = function (parameters) {
            return emWamp.call("/casino#getGames", parameters);
        };

        _service.getGameCategories = function () {
            return emWamp.call("/casino#getGameCategories", { filterByPlatform: null });
        };

        _service.getGameVendors = function () {
            return emWamp.call("/casino#getGameVendors");
        };

        _service.getMostPlayedGames = function (expectedGameFields) {
            return emWamp.call("/casino#mostPlayedGames", { expectedGameFields: expectedGameFields });
        };

        _service.getLastPlayedGames = function (expectedGameFields) {
            return emWamp.call("/casino#lastPlayedGames", { expectedGameFields: expectedGameFields });
        };

        _service.getBiggestWinGames = function (expectedGameFields) {
            return emWamp.call("/casino#biggestWinGames", { expectedGameFields: expectedGameFields });
        };

        _service.getJackpots = function (parameters) {
            return emWamp.call("/casino#getJackpots", parameters);
        };
        // 
        // Query the recommended games by user / game
        //
        // Parameters
        //
        //  {
        //      recommendedType: "user",
        //      slug: [], //mandatory when recommendedType is "game"
        //      platform: "PC",
        //      expectedGameFields: FIELDS.Slug + FIELDS.ShortName // Integer
        //  }
        //
        // recommendedType [string, mandatory]
        // Indicates the type to query the recommended games. Possible values : "user" / "game".
        // 
        // slug [string array, optional]
        // Indicates the game's slugs to query the recommended games. Mandatory when recommendedType is 'game'.
        //
        // platform [string, optional]
        // Indicates the platform to query the recommended games. Platform definition can be found in CasinoEngine API Doc, Appendix E.
        // If this parameter is null, then the games are filtered automatically basing on the client's user-agent string
        //
        // expectedGameFields [integer, mandatory]
        // Indicates the game fields expected in JSON response. Individual field value is defined as below. 
        // Passing this field with the sum of the expected fields' value.  Fields explanation can be found in CasinoEngine API Doc.
        //
        // Return
        // 
        //  {
        //      "games": [{
        //          "rankId": 1,
        //          "game": {
        //              "slug": "wolf-run","shortName": "Wolf Run"
        //          }
        //      },
        //      {
        //          "rankId": 2,
        //          "game": {
        //              "slug": "golden-rush",
        //              "shortName": "Golden Rush"
        //          }
        //      },
        //  .....
        //  ]}
        //
        // games [array]  : Represents the recommended games, the array is sorted in the ascending order of rankId.
        //      rankId [string] : rank ID.
        //      game [JSON object] : the game info
        //
        // The returned game fields are controlled by the expectedGameFields parameter.
        // To get the explanation of the field, please refer to CasinoEngine API Doc.
        //
        _service.getRecommendedGames = function (parameters) {
            return emWamp.call("/casino#getRecommendedGames", parameters);
        };

        _service.getRecommendedGamesUser = function (slug, expectedGameFields) {
            return emWamp.call("/casino#getRecommendedGames",
            {
                recommendedType: "user",
                slug: slug,
                expectedGameFields: expectedGameFields
            });
        };

        _service.getRecommendedGamesGame = function (slug, expectedGameFields) {
            return emWamp.call("/casino#getRecommendedGames",
            {
                recommendedType: "game",
                slug: slug,
                expectedGameFields: expectedGameFields
            });
        };

        //
        // Check the user status and query the url to launch the casino game.
        //
        // Parameters
        //
        //  {
        //      "slug": "",				// String
        //      "tableID": "",			// String
        //      "realMoney": false
        //  }
        //
        // slug  [string, mandatory if for casino]
        // The game slug from /casino#getGames call.
        //
        // tableID  [string, mandatory for live casino]
        // The live casino table id from /casino#getLiveCasinoTables call.
        // 
        // realMoney  [bool, optional]
        // Indicates if launch the game in real money mode or fun mode.
        //
        // Return
        //  {
        //      "status": 0,
        //      "statusText": null,
        //      "url": "http://casino.gammatrix-dev.net/Loader/Start/24/magic-love/?language=en&funMode=True&_sid="
        //  }
        //
        // status  [integer]
        //  Indicates the status code.
        //      0 : Success;  
        //      1 : User is only allowed to withdraw money;
        //      2 : User's profile is incomplete, should redirect user to profile page to complete the profile
        //      3 : The country (either IP or profile country) is blocked for this game
        //      4 : The game is not available for the current user, several reason lists below
        //          game not supported by current terminal type
        //          launch a real money game without login
        //          launch a game in fun mode which does not support fun play.
        //      5 : The email address is not verified, end-user has to click the link in activation email which will cause /user#activate to be called.
        //
        // statusText  [string]
        // The description of the status.
        //
        // url  [string]
        // The game launch url when status equals to zero
        //
        _service.getLaunchUrlRaw = function(parameters) {
            return emWamp.call("/casino#getLaunchUrl", parameters);
        };

        _service.getLaunchUrl = function (slug, tableId, realMoney) {
            return emWamp.call("/casino#getLaunchUrl",
            {
                slug: slug,
                tableID: tableId,
                realMoney: realMoney
            });
        };

        return _service;
    }

})();