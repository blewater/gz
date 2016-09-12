$scope.liveSearch = {
    input: "",
    focused: false,
    searching: false,
    selectedIndex: -1,
    pageSize: 10,
    currentPageIndex: 1,
    totalGameCount: 0,
    totalPageCount: 0,
    games: [],
    noResults: ""
};
function search(index) {
    $scope.liveSearch.searching = true;
    if ($scope.liveSearch.totalPageCount === 0 || index <= $scope.liveSearch.totalPageCount) {
        emCasino.getGames({
            filterByPlatform: null,
            filterByName: [$scope.liveSearch.input],
            //filterBySlug: [$scope.liveSearch.input],
            expectedFields: emCasino.FIELDS.Slug + emCasino.FIELDS.Name + emCasino.FIELDS.Description + emCasino.FIELDS.Thumbnail + emCasino.FIELDS.Logo,
            pageSize: 10,
            pageIndex: index,
        }).then(function (liveSearchResult) {
            $scope.liveSearch.searching = false;
            $scope.liveSearch.showResults = true;
            $scope.liveSearch.currentPageIndex = liveSearchResult.currentPageIndex;
            $scope.liveSearch.totalGameCount = liveSearchResult.totalGameCount;
            $scope.liveSearch.totalPageCount = liveSearchResult.totalPageCount;
            if (index > 1)
                Array.prototype.push.apply($scope.liveSearch.games, liveSearchResult.games);
            else
                $scope.liveSearch.games = liveSearchResult.games;
        }, function () {
            $scope.liveSearch.searching = false;
            console.log(error);
        });
    }
}
$scope.onLiveSearchFocused = function () {
    $scope.liveSearch.focused = true;
    $scope.liveSearch.showResults = $scope.liveSearch.games.length > 0;
};
$scope.onLiveSearchBlurred = function () {
    $scope.liveSearch.focused = false;
};
//$scope.onLiveSearchChange = function () {
//    if ($scope.liveSearch.input.length > 2) {
//        $scope.liveSearch.currentPageIndex = 1;
//        $scope.liveSearch.totalGameCount = 0;
//        $scope.liveSearch.totalPageCount = 0;
//        $scope.liveSearch.games = [];
//        $timeout(function () {
//            search(1);
//        }, 400);
//    }
//};
$scope.onLiveSearchChange = function () {
    $scope.liveSearch.currentPageIndex = 1;
    $scope.liveSearch.totalGameCount = 0;
    $scope.liveSearch.totalPageCount = 0;
    $scope.liveSearch.games = [];
    search(1);
};

$scope.onLoadMore = function () {
    search($scope.liveSearch.currentPageIndex + 1);
};
$scope.selectResult = function (index) {
    for (i = 0; i < $scope.liveSearch.games.length; i++)
        $scope.liveSearch.games[i].selected = i === index;
};
$scope.showResults = function () {
    liveSearch.input.length > 2;
};
