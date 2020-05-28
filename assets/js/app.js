﻿/*!
app.js
(c) 2020 IG PROG, www.igprog.hr
*/
angular.module('app', ['ui.router', 'ngStorage', 'pascalprecht.translate', 'rzSlider'])
.config(['$stateProvider', '$urlRouterProvider', '$httpProvider', '$translateProvider', '$translatePartialLoaderProvider', ($stateProvider, $urlRouterProvider, $httpProvider, $translateProvider, $translatePartialLoaderProvider) => {

    $stateProvider
        .state('home', {
            url: '/', templateUrl: './assets/partials/home.html', controller: 'homeCtrl'
        })
        //.state('shop', {
        //    url: '/shop/:productgroup/:subgroup', params: { pg_code: null }, templateUrl: './assets/partials/shop.html', controller: 'shopCtrl'
        //})
        .state('shop', {
            url: '/shop/', templateUrl: './assets/partials/shop.html', controller: 'shopCtrl'
        })
        .state('category', {
            url: '/category/:productgroup/:subgroup', params: { pg_code: null }, templateUrl: './assets/partials/shop.html', controller: 'shopCtrl'
        })
        .state('search', {
            url: '/search/:search', templateUrl: './assets/partials/shop.html', controller: 'shopCtrl'
        })
        .state('brand', {
            url: '/brand/:brand', params: { brand_code: null }, templateUrl: './assets/partials/shop.html', controller: 'shopCtrl'
        })
        //.state('product', {
        //    url: '/:title_seo', params: { id: null }, templateUrl: './assets/partials/product.html', controller: 'productCtrl'
        //})
        .state('product', {
            url: '/:title_seo/:id', templateUrl: './assets/partials/product.html', controller: 'productCtrl'
        })
        .state('cart', {
            url: '/cart', templateUrl: './assets/partials/cart.html', controller: 'cartCtrl'
        })
        .state('checkout', {
            url: '/checkout', templateUrl: './assets/partials/checkout.html', controller: 'checkoutCtrl'
        })
        .state('about', {
            url: '/about', templateUrl: './assets/partials/about.html', controller: 'aboutCtrl'
        })
        .state('contact', {
            url: '/contact', templateUrl: './assets/partials/contact.html', controller: 'contactCtrl'
        })

    $urlRouterProvider.otherwise("/");

    $translateProvider.useLoader('$translatePartialLoader', {
        urlTemplate: '../assets/json/translations/{lang}/{part}.json'
    });
    $translateProvider.preferredLanguage('hr');
    $translatePartialLoaderProvider.addPart('main');
    $translateProvider.useSanitizeValueStrategy('escape');

    //*******************disable catche**********************
    if (!$httpProvider.defaults.headers.get) {
        $httpProvider.defaults.headers.get = {};
    }
    $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
    $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
    $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
    //*******************************************************
}])

.factory('f', ['$http', ($http) => {
    return {
        post: (service, method, data) => {
            return $http({
                url: '../' + service + '.asmx/' + method,
                method: 'POST',
                data: data
            })
            .then((response) => {
                return JSON.parse(response.data.d);
            },
            (response) => {
                return response.data.d;
            });
        },
        setDate: (x) => {
            var day = x.getDate();
            day = day < 10 ? '0' + day : day;
            var mo = x.getMonth();
            mo = mo + 1 < 10 ? '0' + (mo + 1) : mo + 1;
            var yr = x.getFullYear();
            return yr + '-' + mo + '-' + day;
        },
        setDateTime: (x) => {
            debugger;
            var day = x.getDate();
            day = day < 10 ? '0' + day : day;
            var mo = x.getMonth();
            mo = mo + 1 < 10 ? '0' + (mo + 1) : mo + 1;
            var yr = x.getFullYear();
            var h = x.getHours();
            h = h < 10 ? '0' + h : h;
            var m = x.getMinutes();
            m = m < 10 ? '0' + m : m;
            return day + '.' + mo + '.' + yr + ' - ' + h + ':' + m;
        },
        sticker: (x) => {
            var a = {
                style: null,
                title: null
            }
            if (x.outlet) {
                a.style = 'type-2';
                a.title = 'akcija';
            } else if (x.isnew) {
                a.style = 'type-1';
                a.title = 'novo';
            } else {
                a.style = null;
                a.title = null;
            }
            return a;
        }
    }
}])

.controller('appCtrl', ['$scope', '$http', '$rootScope', 'f', '$sessionStorage', '$translate', '$translatePartialLoader', '$state', '$localStorage', function ($scope, $http, $rootScope, f, $sessionStorage, $translate, $translatePartialLoader, $state, $localStorage) {

    //$scope.shop = (code, productgroup, subgroup) => {
    //    $state.go('shop', { pg_code: code, productgroup: productgroup, subgroup: subgroup });
    //}
    $scope.shop = (pg_code, productgroup, subgroup) => {
        //$state.go('shop', { pg_code: pg_code, productgroup: productgroup, subgroup: subgroup });
        $state.go('shop');
    }

    $scope.goCategory = (pg_code, productgroup, subgroup) => {
        $state.go('category', { pg_code: pg_code, productgroup: productgroup, subgroup: subgroup });
    }

    $scope.search = (search) => {
        debugger;
        $state.go('search', { search: search });
    }

    $scope.brand = (brand_code, brand_seo) => {
        debugger;
        $state.go('brand', { brand_code: brand_code, brand: brand_seo });
    }

    $scope.home = () => {
        $state.go('home');
    }

    //$scope.go = (x, param) => {
    //    $state.go(x, { name: param });
    //}
    $scope.go = (x) => {
        $state.go(x);
    }
    $state.go('home');

    $scope.get = (x) => {
        debugger;
        $state.go('product', { title_seo: x.title_seo, id: x.id });
    }

    var queryString = location.search;
    var params = queryString.split('&');

    if (params.length > 1) {
        $scope.page = parseInt(params[1].substring(5, 6));
    } else {
        $scope.page = 1;
    }

    if ($sessionStorage.filters !== undefined) {
        $sessionStorage.filters = null;
    }

    var data = {
        loading: false,
        productGroups: null,
        brands: null,
        outlet: [],
        newproducts: [],
        bestselling: [],
        records: [],
        cart: null,
        search: null,
        info: null,
        lastReviews: null,
        stars: [1, 2, 3, 4, 5]
    }
    $rootScope.d = data;

    /***** Cart *****/
    var initCart = () => {
        $scope.d.loading = true;
        f.post('Cart', 'Init', {}).then((d) => {
            $rootScope.d.cart = d;
            $scope.d.loading = false;
        });
    }

    if (localStorage.cart != undefined && localStorage.cart != 'undefined' && localStorage.cart != '') {
        $rootScope.d.cart = JSON.parse(localStorage.cart);
    } else {
        initCart();
    }

    $scope.addToCart = (x) => {
        debugger;
        $scope.d.loading = true;
        f.post('Cart', 'AddToCart', { cart: $rootScope.d.cart, x: x }).then((d) => {
            $rootScope.d.cart = d;
            localStorage.cart = JSON.stringify(d);
            $scope.d.loading = false;
        });
    }

    $scope.calcTotPrice = (x) => {
        debugger;
        f.post('Cart', 'CalcTotPrice', { cart: x }).then((d) => {
            $scope.d.cart = d;
            $rootScope.d.cart = d;
            localStorage.cart = JSON.stringify(d);
            location.reload();
        });
    }

    $scope.removeCartItem = function (x, idx) {
        debugger;
        $scope.d.cart.items.splice(idx, 1);
        //$rootScope.d.cart.items.splice(idx, 1);
        $scope.calcTotPrice($scope.d.cart);
        
    }
    /***** Cart *****/

    var loadProductGroups = () => {
        $scope.d.loading = true;
        f.post('ProductGroups', 'Load', {}).then((d) => {
            $scope.d.productGroups = d;
            $rootScope.d.productGroups = d;
            $scope.d.loading = false;
        });
    }

    var loadBrands = () => {
        $scope.d.loading = true;
        f.post('Brands', 'Load', {}).then((d) => {
            $scope.d.brands = d;
            $scope.d.loading = false;
        });
    }

    var loadOutlet = (lang, pg, limit) => {
        $scope.d.loading = true;
        f.post('Products', 'LoadProductType', { lang: lang, productGroup: pg, type: 'outlet', limit: limit }).then((d) => {
            $scope.d.outlet = d;
            $scope.d.loading = false;
        });
    }

    var loadNewProducts = (lang, pg, limit) => {
        $scope.d.loading = true;
        f.post('Products', 'LoadProductType', { lang: lang, productGroup: pg, type: 'isnew', limit: limit }).then((d) => {
            $scope.d.newproducts = d;
            $scope.d.loading = false;
        });
    }

    var loadInfo = (lang) => {
        f.post('Info', 'Load', { lang: lang }).then((d) => {
            //$rootScope.info = d;
            $scope.d.info = d;
        });
    }

    var loadLastReviews = (lang, limit) => {
        f.post('Review', 'LoadLastReviews', { lang: lang, limit: limit }).then((d) => {
            $scope.d.lastReviews = d;
        });
    }
    loadLastReviews('hr', 3);

    var loadData = () => {
        loadProductGroups();
        loadBrands();
        loadOutlet('hr', null, 4);
        loadNewProducts('hr', null, 4);
        loadInfo($rootScope.lang);
    }

    var getConfig = function () {
        $http.get('../config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
              $sessionStorage.config = response.data;
              $sessionStorage.lang = response.data.lang.code;
              $rootScope.lang = $sessionStorage.lang;
              /*** lang ***/
              var queryString = location.search;
              if (queryString !== '') {
                  var params = queryString.split('&');
                  if (params.length == 1) {
                      $sessionStorage.lang = params[0].substring(6, 8);
                      $rootScope.lang = $sessionStorage.lang;
                      $translate.use($sessionStorage.lang);
                      $translatePartialLoader.addPart('main');
                  }
              }
              /*** lang ***/

              /*** reload page ***/
              if (typeof (Storage) !== 'undefined') {
                  if (localStorage.version) {
                      if (localStorage.version !== $scope.config.version) {
                          localStorage.version = $scope.config.version;
                          window.location.reload(true);
                          loadData();
                      } else {
                          loadData();
                      }
                  } else {
                      localStorage.version = $scope.config.version;
                      loadData();
                  }
              } else {
                  loadData();
              }
              /*** reload page ***/
          });
    };
    getConfig();

    $scope.setLang = function (x) {
        $rootScope.config.lang = x;
        $sessionStorage.lang = x.code;
        $rootScope.lang = $sessionStorage.lang;
        $translate.use(x.code);
        $translatePartialLoader.addPart('main');
        //window.location.href = window.location.origin + '?lang=' + x.code;
        loadInfo(x.code);
    };

    $scope.tick = 0;
    var getTime = () => {
        var d = new Date();
        $scope.tick = d.getTime();
    }
    getTime();

}])

.controller('homeCtrl', ['$scope', '$http', '$rootScope', 'f', '$sessionStorage', '$translate', '$stateParams', function ($scope, $http, $rootScope, f, $sessionStorage, $translate, $stateParams) {

    var data = {
        loading: false,
        productGroups: null,
        bestselling: [],
        records: [],
        info: null,
        mainGallery: null,
        services: []
    }
    $scope.d = data;

    var loadProductGroups = () => {
        $scope.d.loading = true;
        f.post('ProductGroups', 'Load', {}).then((d) => {
            $scope.d.productGroups = d;
            $scope.d.loading = false;
        });
    }
    loadProductGroups();

    var loadBestSelling = (lang, pg, limit) => {
        $scope.d.loading = true;
        f.post('Products', 'LoadProductType', { lang: lang, productGroup: pg, type: 'bestselling', limit: limit }).then((d) => {
            $scope.d.bestselling = d;
            $scope.d.loading = false;
        });
    }
    loadBestSelling('hr', null, 4);

}])

.controller('shopCtrl', ['$scope', '$http', '$rootScope', 'f', '$sessionStorage', '$translate', '$state', '$stateParams', function ($scope, $http, $rootScope, f, $sessionStorage, $translate, $state, $stateParams) {

    //$scope.search = { price_min: '', price_max: '', amount_min: 1000, amount_max: 5000 };

    $scope.slider = {
        minValue: 0,
        maxValue: 0,
        options: {
            floor: 0,
            ceil: 100,
            step: 1,
            minRange: 10
        }
    };

    var data = {
        loading: false,
        productGroups: null,
        records: [],
        pg_code: $stateParams.pg_code,
        productgroup: $stateParams.productgroup,
        subgroup: $stateParams.subgroup,
        search: $stateParams.search,
        responseTime: 0,
        filters: $sessionStorage.filters !== undefined ? $sessionStorage.filters : null
    }
    debugger;
    $scope.d = data;
    //$scope.d.search = $scope.search_;

    var loadProductGroups = () => {
        f.post('ProductGroups', 'Load', {}).then((d) => {
            $scope.d.productGroups = d;
        });
    }
    loadProductGroups();

    var load = (param) => {
        if ($sessionStorage.filters !== undefined) { $sessionStorage.filters = null; };
        var pg_code = param.pg_code !== undefined ? param.pg_code : null;
        var brand_code = param.brand_code !== undefined ? param.brand_code : null;
        var search = param.search !== undefined ? param.search : null;

        $scope.d.loading = true;
        f.post('Products', 'Load', { lang: 'hr', productGroup: pg_code, brand: brand_code, search: search }).then((d) => {
            $scope.d.records = d.data;
            //$scope.d.priceRange = d.priceRange;
            $scope.d.filters = d.filters;
            //$scope.d.sortTypes = d.sortTypes;

            $scope.slider.minValue = $scope.d.filters.price.min;
            $scope.slider.maxValue = $scope.d.filters.price.max;
            $scope.slider.options.ceil = $scope.d.filters.price.max;

            $scope.d.responseTime = d.responseTime;
            if (search !== null) {
                $scope.d.search = search;
            }
            $scope.d.loading = false;
        });
    }

    $scope.filter = (filters, slider) => {
        var param = $stateParams;
        var pg_code = param.pg_code !== undefined ? param.pg_code : null;
        var brand_code = param.brand_code !== undefined ? param.brand_code : null;
        var search = param.search !== undefined ? param.search : null;
        //$scope.d.filters.price.min = slider.minValue;
        //$scope.d.filters.price.max = slider.maxValue;
        debugger;
        
        filters.price.minVal = slider.minValue;
        filters.price.maxVal = slider.maxValue;
        //$stateParams.filters = filters;
        $sessionStorage.filters = filters;
        $scope.d.loading = true;
        f.post('Products', 'Filter', { lang: 'hr', productGroup: pg_code, brand: brand_code, search: search, filters: filters}).then((d) => {
            $scope.d.records = d.data;
            $scope.d.loading = false;
        });
    }


    if ($sessionStorage.filters !== undefined) {
        if ($sessionStorage.filters !== null) {
            $scope.slider.minValue = $sessionStorage.filters.price.min;
            $scope.slider.maxValue = $sessionStorage.filters.price.max;
            $scope.slider.options.ceil = $sessionStorage.filters.price.max;
        }
    }
    if ($scope.d.filters) {
        $scope.filter($scope.d.filters, $scope.slider);
    } else {
        load($stateParams);
    }

    var loadBestSelling = (lang, pg, limit) => {
        var pg_code = pg !== undefined ? pg : null;
        f.post('Products', 'LoadProductType', { lang: lang, productGroup: pg_code, type: 'bestselling', limit: limit }).then((d) => {
            $scope.d.bestselling = d;
        });
    }
    loadBestSelling('hr', $stateParams.pg_code, 3);


    // Staviti u functions
    $scope.sticker = (x) => {
        return f.sticker(x);
        //var a = {
        //    style: null,
        //    title: null
        //}
        //if (x.outlet) {
        //    a.style = 'type-2';
        //    a.title = 'akcija';
        //} else if (x.isnew) {
        //    a.style = 'type-1';
        //    a.title = 'novo';
        //} else {
        //    a.style = null;
        //    a.title = null;
        //}
        //return a;
    }

}])

.controller('productCtrl', ['$scope', '$http', '$rootScope', 'f', '$sessionStorage', '$translate', '$state', '$stateParams', '$localStorage', function ($scope, $http, $rootScope, f, $sessionStorage, $translate, $state, $stateParams, $localStorage) {

    var data = {
        loading: false,
        productGroups: null,
        bestselling: [],
        bestsellingall: [],
        record: [],
        review: null,
        tpl: 'descTpl',
        stars: [1,2,3,4,5]
    }
    $scope.d = data;

    var loadProductGroups = () => {
        $scope.d.loading = true;
        f.post('ProductGroups', 'Load', {}).then((d) => {
            $scope.d.productGroups = d;
            $scope.d.loading = false;
        });
    }
    loadProductGroups();

    var loadBestSelling = (lang, pg, limit) => {
        $scope.d.loading = true;
        f.post('Products', 'LoadProductType', { lang: lang, productGroup: pg, type: 'bestselling', limit: limit }).then((d) => {
            $scope.d.bestselling = d;
            $scope.d.loading = false;
        });
    }

    var loadBestSellingAll = (lang, pg, limit) => {
        $scope.d.loading = true;
        f.post('Products', 'LoadProductType', { lang: lang, productGroup: pg, type: 'bestselling', limit: limit }).then((d) => {
            $scope.d.bestsellingall = d;
            $scope.d.loading = false;
        });
    }

    var get = (id) => {
        $scope.d.loading = true;
        f.post('Products', 'Get', { id: id, lang: 'hr' }).then((d) => {
            $scope.d.record = d;
            initReview(d.sku);
            loadBestSelling('hr', $scope.d.record.productGroup.code, 3);
            loadBestSellingAll('hr', null, 4);
            $scope.d.loading = false;
        });
    }
    get($stateParams.id);

    $scope.mainImgIdx = 0;
    $scope.selectImg = function (idx) {
        $scope.mainImgIdx = idx;
    }

    $scope.sticker = (x) => {
        return f.sticker(x);
    }

    $scope.toggleTpl = (x) => {
        $scope.d.tpl = x;
    }

    /**** Review & Rating *****/
    $scope.getRate = function (rate) {
        $scope.d.review.rating = rate;
    }
    //$scope.rating = 0;

    var initReview = (sku) => {
        f.post('Review', 'Init', {sku: sku}).then((d) => {
            $scope.d.review = d;
        });
    }

    $scope.saveRating = (x) => {
        if (x.rating < 1) {
            alert($translate.instant('please rate us'));
            return false;
        }
        x.reviewdate = f.setDateTime(new Date());
        x.lang = 'hr';
        f.post('Review', 'Save', { x: x }).then((d) => {
            $scope.d.record.reviews = d;
        });
    }

    $scope.reviewsTrans = (x) => {
        lang = 'hr';
        if (lang === 'en') {
            return x < 2 ? 'review' : 'reviews';
        } else {
            if (x < 2) {
                return 'review';
            } else if ((x % 10 == 2) || (x % 10 == 3) || (x % 10 == 4)) {
                return 'reviews';
            } else {
                return 'review';
            }
        }
    }
    /**** Review & Rating *****/
   

}])

.controller('cartCtrl', ['$scope', '$http', '$rootScope', 'f', '$sessionStorage', '$translate', '$state', '$stateParams', '$localStorage', function ($scope, $http, $rootScope, f, $sessionStorage, $translate, $state, $stateParams, $localStorage) {

    var data = {
        cart: [],
        bestsellingall: null
    }
    $scope.d = data;

    debugger;
    if (localStorage.cart != undefined && localStorage.cart != 'undefined' && localStorage.cart != '') {
        $rootScope.d.cart = JSON.parse(localStorage.cart);
        $scope.d.cart = $rootScope.d.cart;
        //TODO: calculate total price
    } else {
        $rootScope.cart = [];
        $scope.cart = [];
        localStorage.cart = [];
    }

    $scope.calcItemPrice = (x, idx) => {
        f.post('Cart', 'CalcItemPrice', { item: x.items[idx] }).then((d) => {
            debugger;
            $scope.d.cart.items[idx] = d;
            $scope.calcTotPrice(x);
            //localStorage.cart = JSON.stringify($scope.d.cart);
        });
    }

    $scope.calcTotPrice = (x) => {
        debugger;
        f.post('Cart', 'CalcTotPrice', { cart: x }).then((d) => {
            $scope.d.cart = d;
            $rootScope.d.cart = d;
            localStorage.cart = JSON.stringify(d);
        });
    }

    $scope.removeItem = function (x, idx) {
        debugger;
        $scope.d.cart.items.splice(idx, 1);
        //$rootScope.d.cart.items.splice(idx, 1);
        $scope.calcTotPrice($scope.d.cart);
    }

    $scope.clearCart = function () {
        localStorage.clear();
        f.post('Cart', 'Init', {}).then((d) => {
            $scope.d.cart = d;
            $rootScope.d.cart = d;
        });
    }

    var loadBestSellingAll = (lang, pg, limit) => {
        $scope.d.loading = true;
        f.post('Products', 'LoadProductType', { lang: lang, productGroup: pg, type: 'bestselling', limit: limit }).then((d) => {
            $scope.d.bestsellingall = d;
            $scope.d.loading = false;
        });
    }
    loadBestSellingAll('hr', null, 4);

}])

.controller('checkoutCtrl', ['$scope', '$http', '$rootScope', 'f', '$sessionStorage', '$translate', '$state', '$stateParams', '$localStorage', function ($scope, $http, $rootScope, f, $sessionStorage, $translate, $state, $stateParams, $localStorage) {

    var data = {
        order: []
    }
    $scope.d = data;

    var init = (x) => {
        f.post('Orders', 'Init', { cart: x }).then((d) => {
            debugger;
            $scope.d.order = d;
        });
    }
    init($rootScope.d.cart);

    var clearCart = function () {
        localStorage.clear();
        f.post('Cart', 'Init', {}).then((d) => {
            $scope.d.cart = d;
            $rootScope.d.cart = d;
        });
    }

    $scope.confirm = (x) => {
        f.post('Orders', 'Confirm', { x: x }).then((d) => {
            $scope.d.order = d;
            // clear cart
            if (d.response.isSuccess) {
                clearCart();
            }
        });
    }

}])

.controller('detailsCtrl', ['$scope', '$http', '$rootScope', 'f', '$sessionStorage', '$translate', function ($scope, $http, $rootScope, f, $sessionStorage, $translate) {
    var queryString = location.search;
    var params = queryString.split('&');
    var id = null;
    $scope.lang = $sessionStorage.lang;
    $scope.loading = false;
    if (params.length > 0) {
        if (params[0].substring(1, 3) === 'id') {
            id = params[0].substring(4);
        }
        if (params.length > 1) {
            /*** lang ***/
            if (params[1].substring(0, 4) === 'lang') {
                $scope.lang = params[1].substring(5, 7);
                $translate.use($scope.lang);
            }
            /*** lang ***/
        }
    }

    var get = (id) => {
        if (id == null) { return false;}
        $scope.loading = true;
        f.post('Products', 'Get', { id: id, lang: $scope.lang }).then((d) => {
            $scope.d = d;
            $scope.loading = false;
        });
    }
    get(id);

}])

.controller('aboutCtrl', ['$scope', '$http', '$rootScope', 'f', '$translate', function ($scope, $http, $rootScope, f, $translate) {
    var data = {
        info: []
    }
    $scope.d = data;

    var load = (lang) => {
        f.post('Info', 'Load', { lang: lang }).then((d) => {
            $scope.d.info = d;
        });
    }
    load('hr');

    }])

.controller('contactCtrl', ['$scope', '$http', '$rootScope', 'f', '$translate', function ($scope, $http, $rootScope, f, $translate) {
    var service = 'Contact';
    $scope.loading = false;

    var init = () => {
        f.post(service, 'Init', {}).then((d) => {
            $scope.d = d;
        });
    }
    init();

    $scope.send = function (d) {
        $scope.loading = true;
        f.post(service, 'Send', { x: d }).then((d) => {
            $scope.d = d;
            $scope.loading = false;
        })
    }

    $scope.upload = (d) => {
        var content = new FormData(document.getElementById('formUpload_'));
        $http({
            url: '../UploadHandler.ashx',
            method: 'POST',
            headers: { 'Content-Type': undefined },
            data: content,
        }).then(function (response) {
            $scope.d.file = response.data;
        },
        function (response) {
            alert(response.data.d);
        });
    }

    $scope.removeImg = function () {
        $scope.d.file = null;
    }

}])

//.controller('postsCtrl', ['$scope', '$http', '$rootScope', 'f', '$translate', '$timeout', function ($scope, $http, $rootScope, f, $translate, $timeout) {
//    var service = 'Products';
//    var data = {
//        loading: false,
//        records: [],
//        info: null,
//        mainGallery: null,
//        services: []
//    }
//    $scope.d = data;


//    var loadPosts = (lang) => {
//        $scope.d.loading = true;
//        f.post(service, 'Load', { lang: lang, order: true, productGroupId: $rootScope.config.postsId }).then((d) => {
//            $scope.d.records = d;
//            $scope.d.loading = false;
//        });
//    }
//    $timeout(function () {
//        loadPosts($rootScope.lang);
//    }, 500);

//}])

/********** Directives **********/
//.directive('reservationDirective', () => {
//    return {
//        restrict: 'E',
//        scope: {
//            service: '='
//        },
//        templateUrl: '../assets/partials/reservation.html'
//    };
//})

//.directive('detailsDirective', () => {
//    return {
//        restrict: 'E',
//        scope: {
//            id: '=',
//            product: '=',
//            shortdesc: '=',
//            longdesc: '=',
//            img: '=',
//            price: '=',
//            gallery: '=',
//            options: '='
//        },
//        templateUrl: '../assets/partials/directive/details.html'
//    };
//})

.directive('pgDirective', () => {
    return {
        restrict: 'E',
        scope: {
            pg: '=',
            record: '='
        },
        templateUrl: '../assets/partials/directive/pgcategories.html',
        controller: 'pgCtrl'
    };
})
.controller('pgCtrl', ['$scope', '$state', ($scope, $state) => {
    $scope.goCategory = (pg_code, productgroup, subgroup) => {
        $state.go('category', { pg_code: pg_code, productgroup: productgroup, subgroup: subgroup });
    }
}])

.directive('bestsellingDirective', () => {
    return {
        restrict: 'E',
        scope: {
            data: '=',
            record: '='
        },
        templateUrl: '../assets/partials/directive/bestselling.html',
        controller: 'bestsellingCtrl'
    };
})
.controller('bestsellingCtrl', ['$scope', 'f', '$rootScope', '$state', ($scope, f, $rootScope, $state) => {
    $scope.get = (x) => {
        $state.go('product', { title_seo: x.title_seo, id: x.id });
    }

    $scope.addToCart = (x) => {
        f.post('Cart', 'AddToCart', { cart: $rootScope.d.cart, x: x }).then((d) => {
            $rootScope.d.cart = d;
            localStorage.cart = JSON.stringify(d);
        });
    }

    $scope.sticker = (x) => {
        return f.sticker(x);
        //var a = {
        //    style: null,
        //    title: null
        //}
        //if (x.outlet) {
        //    a.style = 'type-2';
        //    a.title = 'akcija';
        //} else if (x.isnew) {
        //    a.style = 'type-1';
        //    a.title = 'novo';
        //} else {
        //    a.style = null;
        //    a.title = null;
        //}
        //return a;
    }

}])


//.directive('detailsDirective', () => {
//    return {
//        restrict: 'E',
//        scope: {
//            data: '='
//        },
//        templateUrl: '../assets/partials/directive/details.html'
//    };
//})

//.directive('navbarDirective', () => {
//    return {
//        restrict: 'E',
//        scope: {
//            site: '=',
//            lang: '='
//        },
//        templateUrl: '../assets/partials/directive/navbar.html'
//    };
//})

.directive('cardDirective', () => {
    return {
        restrict: 'E',
        scope: {
            data: '=',
            link: '=',
            showdesc: '=',
            lang: '='
        },
        templateUrl: '../assets/partials/directive/card.html'
    };
})

.directive('loadingDirective', () => {
    return {
        restrict: 'E',
        scope: {
            loadingtitle: '=',
            val: '=',
            size: '='
        },
        templateUrl: '../assets/partials/directive/loading.html'
    };
})

.directive('footerDirective', () => {
    return {
        restrict: 'E',
        scope: {
            sitename: '='
        },
        templateUrl: '../assets/partials/directive/footer.html',
        controller: 'footerCtrl'
    };
})
.controller('footerCtrl', ['$scope', '$translate', ($scope, $translate) => {
    $scope.year = (new Date).getFullYear();
}])

//.directive('corouselDirective', () => {
//    return {
//        restrict: 'E',
//        scope: {
//            showdes: '='
//        },
//        templateUrl: '../assets/partials/directive/corousel.html',
//        controller: 'corouselCtrl'
//    };
//})
//.controller('corouselCtrl', ['$scope', ($scope) => {
//    $scope.tick = 0;
//    var getTime = () => {
//        var d = new Date();
//        $scope.tick = d.getTime();
//    }
//    getTime();
//}])

//.directive('galleryDirective', () => {
//    return {
//        restrict: 'E',
//        scope: {
//            data: '='
//        },
//        templateUrl: '../assets/partials/directive/gallery.html',
//        controller: 'galleryCtrl'
//    };
//})
//.controller('galleryCtrl', ['$scope', '$translate', '$mdDialog', ($scope, $translate, $mdDialog) => {

//    var openPopup = function (x, idx) {
//        if ($(window).innerWidth() < 560) { return false; }
//        $mdDialog.show({
//            controller: popupCtrl,
//            templateUrl: '../assets/partials/popup/gallery.html',
//            parent: angular.element(document.body),
//            clickOutsideToClose: true,
//            d: { data: x, idx: idx }
//        })
//       .then(function (x) {
//       }, function () {
//       });
//    }

//    var popupCtrl = function ($scope, $mdDialog, $http, d, f) {
//        $scope.d = d;

//        $scope.back = (idx) => {
//            if (idx > 0) {
//                $scope.d.idx = idx - 1;
//            }
//        }

//        $scope.forward = (idx) => {
//            if (idx >= 0 && idx < $scope.d.data.gallery.length - 1) {
//                $scope.d.idx = idx + 1;
//            }
//        }

//        $scope.cancel = function () {
//            $mdDialog.cancel();
//        };
//    };

//    $scope.openPopup = (x, idx) => {
//        return openPopup(x, idx);
//    }

//}])

//.directive('servicesDirective', () => {
//    return {
//        restrict: 'E',
//        scope: {
//            data: '='
//        },
//        templateUrl: '../assets/partials/directive/services.html'
//    };
//})

//.directive('postDirective', () => {
//    return {
//        restrict: 'E',
//        scope: {
//            data: '='
//        },
//        templateUrl: '../assets/partials/directive/post.html'
//    };
//})

.directive('jsonDirective', function () {
    return {
        restrict: 'E',
        scope: {
            data: '=',
            desc: '=',
            debug: '='
        },
        templateUrl: './assets/partials/directive/json.html',
        controller: 'jsonCtrl'
    };
})

.controller('jsonCtrl', ['$scope', '$rootScope', function ($scope, $rootScope) {
    $scope.isShow = false;
    $scope.show = function () {
        $scope.isShow = !$scope.isShow;
    }
}])


.directive('allowOnlyNumbers', function () {
    return {
        restrict: 'A',
        link: function (scope, elm, attrs, ctrl) {
            elm.on('keydown', function (event) {
                var $input = $(this);
                var value = $input.val();
                value = value.replace(',', '.');
                $input.val(value);
                if (event.which == 64 || event.which == 16) {
                    return false;
                } else if (event.which >= 48 && event.which <= 57) {
                    return true;
                } else if (event.which >= 96 && event.which <= 105) {
                    return true;
                } else if ([8, 13, 27, 37, 38, 39, 40].indexOf(event.which) > -1) {
                    return true;
                } else if (event.which == 110 || event.which == 188 || event.which == 190) {
                    return true;
                } else if (event.which == 46) {
                    return true;
                } else {
                    event.preventDefault();
                    return false;
                }
            });
        }
    }
})

.directive('starating', function ($compile) {
    return {
        template: '<div></div>',
        replace: true,
        link: function (scope, element) {
            scope.rating = 0;
            var el = angular.element('<span/>');
            var max = 5;
            var value = 0;
            var stars = []
            for (var i = 1; i <= max; i++) {
                var star = angular.element('<span data-id="' + i + '" class="glyphicon glyphicon-star-empty" ng-click="rating=' + i + ';getRate(' + i + ')"></span>');
                stars.push(star);
                el.append(star);
                star.bind('mouseover', function () {
                    this.classList.remove('glyphicon-star-empty');
                    this.classList.add('glyphicon-star');
                    value = angular.element(this).attr('data-id');
                    updateStars(value);
                }).bind('mouseout', function () {
                    updateStars(value);
                });
            }
            function updateStars(val) {
                for (var j = 0; j < max; j++) {
                    if (stars[j].attr('data-id') <= val) {
                        stars[j].removeClass("glyphicon-star-empty");
                        stars[j].addClass("glyphicon-star");
                    } else {
                        stars[j].removeClass("glyphicon-star");
                        stars[j].addClass("glyphicon-star-empty");
                    }
                }
            }
            el.bind('mouseout', function () {
                updateStars(scope.rating);
            });
            $compile(el)(scope);
            element.append(el);
        }
    }
})
/********** Directives **********/

;