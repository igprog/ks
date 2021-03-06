﻿/*!
app.js
(c) 2020 - 2021 IG PROG, www.igprog.hr
*/
angular.module('app', ['ui.router', 'ngStorage', 'pascalprecht.translate', 'rzSlider', 'ui.bootstrap', 'slick', 'ngSanitize'])
.config(['$stateProvider', '$urlRouterProvider', '$httpProvider', '$translateProvider', '$translatePartialLoaderProvider', ($stateProvider, $urlRouterProvider, $httpProvider, $translateProvider, $translatePartialLoaderProvider) => {

    $stateProvider
        .state('home', {
            url: '/', templateUrl: './assets/partials/home.html', controller: 'homeCtrl'
        })
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
        .state('type', {
            url: '/type/:type', templateUrl: './assets/partials/shop.html', controller: 'shopCtrl'
        })
        .state('product', {
            url: '/:title_seo/:sku', templateUrl: './assets/partials/product.html', controller: 'productCtrl'
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
        .state('shops', {
            url: '/shops', templateUrl: './assets/partials/shops.html', controller: 'shopsCtrl'
        })
        .state('support', {
            url: '/support', templateUrl: './assets/partials/support.html', controller: 'supportCtrl'
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
        }
    }
}])

.controller('appCtrl', ['$scope', '$http', '$rootScope', 'f', '$sessionStorage', '$translate', '$translatePartialLoader', '$state', '$localStorage', '$stateParams', function ($scope, $http, $rootScope, f, $sessionStorage, $translate, $translatePartialLoader, $state, $localStorage, $stateParams) {

    var data = {
        loading: false,
        productGroups: null,
        brands: null,
        outlet: [],
        newproducts: [],
        records: [],
        cart: null,
        search: null,
        info: null,
        lastReviews: null,
        stars: [1, 2, 3, 4, 5],
        imgFolder: 'productgroups',
        autoscroll: true,
        subscribe: null
    }
    $rootScope.d = data;

    $scope.shop = (pg_code, productgroup, subgroup) => {
        $state.go('shop');
    }

    $scope.goCategory = (pg_code, productgroup, subgroup) => {
        $state.go('category', { pg_code: pg_code, productgroup: productgroup, subgroup: subgroup });
        /***** Hide menu on mobile after click *****/
        $(".pg-mobile").on("click", function () {
            $(".header-navigation").hide();
        });
        /***** Hide menu on mobile after click *****/
    }

    $rootScope.search = (search) => {
        $stateParams.pg_code = null;
        $stateParams.productgroup = null;
        $stateParams.subgroup = null;
        $sessionStorage.pg_code = null;
        $state.go('search', { search: search });
    }

    $scope.brand = (brand_code, brand_seo) => {
        $state.go('brand', { brand_code: brand_code, brand: brand_seo });
    }

    $scope.goType = (type) => {
        $state.go('type', { type: type });
    }

    $scope.go = (x) => {
        $state.go(x);
    }
    $state.go('home');

    $scope.get = (x) => {
        $state.go('product', { title_seo: x.title_seo, sku: x.sku });
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
        $scope.d.loading = true;
        f.post('Cart', 'AddToCart', { cart: $rootScope.d.cart, x: x }).then((d) => {
            $rootScope.d.cart = d;
            localStorage.cart = JSON.stringify(d);
            $scope.d.loading = false;
        });
    }

    $scope.calcTotPrice = (x) => {
        f.post('Cart', 'CalcTotPrice', { cart: x }).then((d) => {
            $scope.d.cart = d;
            $rootScope.d.cart = d;
            localStorage.cart = JSON.stringify(d);
            location.reload();
        });
    }

    $scope.removeCartItem = function (x, idx) {
        $scope.d.cart.items.splice(idx, 1);
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
            $rootScope.d.info = d;
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

    var initSubscribe = () => {
        f.post('Subscribe', 'Init', {}).then((d) => {
            $scope.d.subscribe = d;
        });
    }
    initSubscribe();

    $scope.subscribe = (x) => {
        if (x.email === null) { return false; }
        f.post('Subscribe', 'Save', { x: x }).then((d) => {
        });
    }

}])

.controller('homeCtrl', ['$scope', '$http', '$rootScope', 'f', '$sessionStorage', '$translate', '$stateParams', '$timeout', function ($scope, $http, $rootScope, f, $sessionStorage, $translate, $stateParams, $timeout) {

    var data = {
        loading: false,
        productGroups: null,
        bestbuy: [],
        specialProductGroup: [],
        records: [],
        info: null,
        mainGallery: null,
        services: [],
        opportunity: [],
        banners: []
    }
    $scope.d = data;

    $rootScope.d.autoscroll = false;

    var loadProductGroups = () => {
        $scope.d.loading = true;
        f.post('ProductGroups', 'Load', {}).then((d) => {
            $scope.d.productGroups = d;
            $scope.d.loading = false;
        });
    }
    loadProductGroups();

    var loadBestBuy = (lang, pg, limit) => {
        f.post('Products', 'LoadProductType', { lang: lang, productGroup: pg, type: 'bestbuy', limit: limit }).then((d) => {
            $scope.d.bestbuy = d;
        });
    }
    loadBestBuy('hr', null, 10);

    var loadSpecialProductGroup = (lang, pg) => {
        f.post('Products', 'Load', { lang: 'hr', productGroup: pg, brand: null, search: null, type: null, isDistinctStyle: true, limit: null }).then((d) => {
            $scope.d.specialProductGroup = d.data;
        });
    }

    var loadInfo = (lang) => {
        f.post('Info', 'Load', { lang: lang }).then((d) => {
            $scope.d.info = d;
            loadSpecialProductGroup(lang, d.specialProductGroup);
        });
    }
    loadInfo('hr');

    var loadOpportunity = (lang, pg, limit) => {
        f.post('Products', 'LoadProductType', { lang: lang, productGroup: pg, type: 'opportunity', limit: limit }).then((d) => {
            $scope.d.opportunity = d;
        });
    }
    loadOpportunity('hr', null, 10);

    var loadBanners = () => {
        f.post('Banners', 'Load', {isactive: true}).then((d) => {
            $scope.d.banners = d;
        });
    }
    loadBanners();

}])

.controller('shopCtrl', ['$scope', '$http', '$rootScope', 'f', '$sessionStorage', '$translate', '$state', '$stateParams', function ($scope, $http, $rootScope, f, $sessionStorage, $translate, $state, $stateParams) {
    window.scrollTo(0, 0);

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
        filters: $sessionStorage.filters !== undefined ? $sessionStorage.filters : null,
        isShowFilters: false,  //***** only form mobile *****
        showAllColorBtn: false,
        totRecords: 0,
        totPages: 0,
        pages: [],
        autoscroll: true,
        parentProductGroup: null
    }
    $scope.d = data;

    $rootScope.d.autoscroll = false;

    var loadProductGroups = () => {
        f.post('ProductGroups', 'Load', {}).then((d) => {
            $scope.d.productGroups = d;
        });
    }
    loadProductGroups();

    var load = (param) => {
        if ($sessionStorage.filters !== undefined) { $sessionStorage.filters = null; };
        var pg_code = param.pg_code !== undefined ? param.pg_code : null;
        if ($sessionStorage.pg_code !== undefined) {
            if ($sessionStorage.pg_code !== null && pg_code === null) {
                pg_code = $sessionStorage.pg_code;
            }
        }
        $sessionStorage.pg_code = pg_code;
        var brand_code = param.brand_code !== undefined ? param.brand_code : null;
        var search = param.search !== undefined ? param.search : null;
        var type = param.type !== undefined ? param.type : null;

        $scope.d.loading = true;
        f.post('Products', 'Load', { lang: 'hr', productGroup: pg_code, brand: brand_code, search: search, type: type, isDistinctStyle: false, limit: $rootScope.config.limit }).then((d) => {
            $scope.d.records = d.data;
            $scope.d.filters = d.filters;
            $scope.d.totRecords = d.totRecords;
            $scope.d.parentProductGroup = d.parentProductGroup;
            setTotPages();

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
        var type = param.type !== undefined ? param.type : null;

        filters.price.minVal = slider.minValue;
        filters.price.maxVal = slider.maxValue;
        $sessionStorage.filters = filters;
        $scope.d.loading = true;
        f.post('Products', 'Filter', { lang: 'hr', productGroup: pg_code, brand: brand_code, search: search, type: type, filters: filters}).then((d) => {
            $scope.d.records = d.data;
            $scope.d.totRecords = d.totRecords;
            setTotPages();
            $scope.d.loading = false;
        });
    }

    $scope.search = (search) => {
        $rootScope.search(search);
    }

    var setTotPages = () => {
        $scope.d.totPages = Math.ceil($scope.d.totRecords / $scope.d.filters.show.val);
        $scope.d.pages = [];
        for (var i = 1; i <= $scope.d.totPages; i++) {
            $scope.d.pages.push(i);
        }
    }

    $scope.setCurrPage = (x, slider) => {
        if (x <= 0 || x > $scope.d.totPages) { return false; }
        $scope.d.filters.page = x;
        $scope.filter($scope.d.filters, slider);

    }

    $scope.filterColor = (filters, slider, x) => {
        filters.color.val = x;
        $scope.filter(filters, slider);
        $scope.d.showAllColorBtn = true;
    }


    $scope.clearfilterColor = (filters, slider, x) => {
        var filters_ = angular.copy(filters);
        filters_.color.val.code = null;
        $scope.filter(filters_, slider);
        $scope.d.showAllColorBtn = false;
    }


    if ($sessionStorage.filters !== undefined) {
        if ($sessionStorage.filters !== null) {
            $scope.slider.minValue = $sessionStorage.filters.price.min;
            $scope.slider.maxValue = $sessionStorage.filters.price.max;
            $scope.slider.options.ceil = $sessionStorage.filters.price.max;
        }
    }
    if ($scope.d.filters && $sessionStorage.pg_code === $stateParams.pg_code) {
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

    $scope.showFilters = () => {
        $scope.d.isShowFilters = !$scope.d.isShowFilters;
    }

    $scope.sortBy = (filters, slider) => {
        filters.page = 1;
        $scope.filter(filters, slider);
    }

    $scope.toPage = () => {
        if ($scope.d.filters !== null) {
            if ($scope.d.records.length > $scope.d.filters.show.val) {
                var rest = ($scope.d.filters.show.val * $scope.d.filters.page) % $scope.d.records.length;
                var rest_ = rest < $scope.d.filters.show.val ? rest : 0;
                return ($scope.d.filters.show.val * $scope.d.filters.page) - rest_;
            } else {
                return $scope.d.records.length;
            }
        }
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
        stars: [1, 2, 3, 4, 5],
        activeTab: 'description',
        showColorVar: false,
        showReivewForm: true,
        info: null
    }
    $scope.d = data;
    $rootScope.d.autoscroll = false;

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

    var initReview = (sku) => {
        f.post('Review', 'Init', { sku: sku }).then((d) => {
            $scope.d.review = d;
        });
    }

    var loadInfo = (lang) => {
        f.post('Info', 'Load', { lang: lang }).then((d) => {
            $scope.d.info = d;
        });
    }

    var get = (sku) => {
        $scope.d.loading = true;
        f.post('Products', 'Get', { sku: sku, lang: 'hr' }).then((d) => {
            $scope.d.record = d;
            if (d.styleProducts !== null) {
                if (d.styleProducts.length > 0) {
                    angular.forEach(d.styleProducts, function (val, key) {
                        if (val.color !== null && val.color !== d.color) {
                            $scope.d.showColorVar = true;
                        }
                    });
                }
            }
            loadInfo('hr');
            initReview(d.sku);
            loadBestSelling('hr', $scope.d.record.productGroup.code, 3);
            loadBestSellingAll('hr', null, 4);
            $scope.d.loading = false;
        });
    }
    get($stateParams.sku);

    $scope.getVarDimProduct = (style, dimension) => {
        $scope.d.loading = true;
        f.post('Products', 'GetVarDimProduct', { style: style, dimension: dimension, lang: lang }).then((d) => {
            get(d.sku);
            $scope.d.loading = false;
        });
    }

    $scope.getVarColorProduct = (style, color, dimension) => {
        var dimension_ = angular.copy(JSON.parse(angular.toJson(dimension)));
        $scope.d.loading = true;
        f.post('Products', 'GetVarColorProduct', { style: style, color: color, dimension: dimension_, lang: lang }).then((d) => {
            get(d.sku);
            $scope.d.loading = false;
        });
    }

    $scope.getVarFireboxInsertProduct = (style, fireboxInsert) => {
        $scope.d.loading = true;
        f.post('Products', 'GetVarFireboxInsertProduct', { style: style, fireboxInsert: fireboxInsert, lang: lang }).then((d) => {
            get(d.sku);
            $scope.d.loading = false;
        });
    }

    $scope.mainImgIdx = 0;
    $scope.selectImg = function (idx) {
        if (idx < 0 || idx >= $scope.d.record.gallery.length) { return false; }
        $scope.mainImgIdx = idx;
    }

    $scope.getMainImg = (mainImgIdx) => {
        if ($scope.d.record.length == 0) {
            return false;
        } else {
            if ($scope.d.record.gallery == null) {
                return false;
            } else {
                return '../upload/' + $scope.d.record.id + '/gallery/' + $scope.d.record.gallery[mainImgIdx];
            }
        }
    }

    /**** Review & Rating *****/
    $scope.getRate = function (rate) {
        $scope.d.review.rating = rate;
    }
    //$scope.rating = 0;

    $scope.saveRating = (x) => {
        if (x.rating < 1) {
            alert($translate.instant('please rate product'));
            return false;
        }
        x.reviewdate = f.setDateTime(new Date());
        x.lang = 'hr';
        f.post('Review', 'Save', { x: x }).then((d) => {
            $scope.d.record.reviews = d;
            $scope.d.showReivewForm = false;
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

    $scope.setActiveTab = (x) => {
        $scope.d.activeTab = x;
    }
    /**** Review & Rating *****/
   

}])

.controller('cartCtrl', ['$scope', '$http', '$rootScope', 'f', '$sessionStorage', '$translate', '$state', '$stateParams', '$localStorage', function ($scope, $http, $rootScope, f, $sessionStorage, $translate, $state, $stateParams, $localStorage) {

    var data = {
        cart: [],
        bestsellingall: null
    }
    $scope.d = data;

    $rootScope.d.autoscroll = true;

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
            $scope.d.cart.items[idx] = d;
            $scope.calcTotPrice(x);
        });
    }

    $scope.calcTotPrice = (x) => {
        f.post('Cart', 'CalcTotPrice', { cart: x }).then((d) => {
            $scope.d.cart = d;
            $rootScope.d.cart = d;
            localStorage.cart = JSON.stringify(d);
        });
    }

    $scope.removeItem = function (x, idx) {
        $scope.d.cart.items.splice(idx, 1);
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
        order: [],
        sameAsBillingAddress: true
    }
    $scope.d = data;

    $rootScope.d.autoscroll = true;

    var init = (x) => {
        var method = $rootScope.config.debug ? 'InitTest' : 'Init';
        f.post('Orders', method, { cart: x }).then((d) => {
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
        /**** Validation *****/
        if (!x.confirmTerms) { alert('prohvatite uvjete korištenja'); return false; }
        if (x.user.billingDetails.firstName == '') { alert('ime je obavezno'); return false; }
        if (x.user.billingDetails.lastName == '') { alert('prezime je obavezno'); return false; }
        if (x.user.billingDetails.phone == '') { alert('telefon obavezno'); return false; }
        if (x.user.billingDetails.city == '') { alert('grad je obavezan'); return false; }
        if (x.user.billingDetails.postalCode == '') { alert('poštanski grad je obavezan'); return false; }
        if (x.user.billingDetails.country == '') { alert('država je obavezna'); return false; }
        if (x.user.deliveryDetails.firstName == '') { alert('ime je obavezno (podaci za dostavu)'); return false; }
        if (x.user.deliveryDetails.lastName == '') { alert('prezime je obavezno (podaci za dostavu)'); return false; }
        if (x.user.deliveryDetails.phone == '') { alert('telefon obavezno (podaci za dostavu)'); return false; }
        if (x.user.deliveryDetails.city == '') { alert('grad je obavezan (podaci za dostavu)'); return false; }
        if (x.user.deliveryDetails.postalCode == '') { alert('poštanski grad je obavezan (podaci za dostavu)'); return false; }
        if (x.user.deliveryDetails.country == '') { alert('država je obavezna (podaci za dostavu)'); return false; }
        /**** Validation *****/
        x.orderDate = f.setDateTime(new Date());
        f.post('Orders', 'Confirm', { x: x }).then((d) => {
            $scope.d.order = d;
            // clear cart
            if (d.response.isSuccess) {
                clearCart();
            }
        });
    }

    $scope.changeUserType = (x) => {
        if (x === 'natural') {
            $scope.d.order.user.company = null;
            $scope.d.order.user.deliveryDetails.company = null;
            $scope.d.order.user.pin = null;
            $scope.d.order.user.deliveryDetails.pin = null;
        }
    }

    $scope.setDeliveryAddress = (x) => {
        if (x.sameAsBillingAddress) {
            $scope.d.order.user.deliveryDetails = angular.copy(x.order.user.billingDetails);
        }
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

    var get = (sku) => {
        if (sku == null) { return false;}
        $scope.loading = true;
        f.post('Products', 'Get', { sku: sku, lang: $scope.lang }).then((d) => {
            $scope.d = d;
            $scope.loading = false;
        });
    }
    get(sku);

}])

.controller('aboutCtrl', ['$scope', '$http', '$rootScope', 'f', '$translate', function ($scope, $http, $rootScope, f, $translate) {
    var data = {
        info: []
    }
    $scope.d = data;

    $rootScope.d.autoscroll = true;

    var load = (lang) => {
        f.post('Info', 'Load', { lang: lang }).then((d) => {
            $scope.d.info = d;
        });
    }
    load('hr');

    }])

.controller('contactCtrl', ['$scope', '$http', '$rootScope', 'f', '$translate', function ($scope, $http, $rootScope, f, $translate) {

    $rootScope.d.autoscroll = true;

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

.controller('shopsCtrl', ['$scope', '$http', '$rootScope', 'f', '$translate', function ($scope, $http, $rootScope, f, $translate) {
    var data = {
        info: []
    }
    $scope.d = data;

    $rootScope.d.autoscroll = true;

}])

.controller('supportCtrl', ['$scope', '$http', '$rootScope', 'f', '$translate', function ($scope, $http, $rootScope, f, $translate) {
    var data = {
        info: []
    }
    $scope.d = data;

    $rootScope.d.autoscroll = true;

}])

.controller('CarouselDemoCtrl', ['$scope', function ($scope) {
    $scope.myInterval = 5000;
    var slides = $scope.slides = [];
    $scope.addSlide = function () {
        var newWidth = 600 + slides.length + 1;
        slides.push({
            image: 'http://placekitten.com/' + newWidth + '/300',
            text: ['More', 'Extra', 'Lots of', 'Surplus'][slides.length % 4] + ' ' +
                ['Cats', 'Kittys', 'Felines', 'Cutes'][slides.length % 4]
        });
    };
    for (var i = 0; i < 4; i++) {
        $scope.addSlide();
    }

    $scope.getSecondIndex = function (index) {
        if (index - slides.length >= 0)
            return index - slides.length;
        else
            return index;
    }

}])

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

.directive('specialproductsDirective', () => {
    return {
        restrict: 'E',
        scope: {
            data: '=',
            show: '=',
            showdots: '='
        },
        templateUrl: '../assets/partials/directive/specialproducts.html',
        controller: 'specialproductsCtrl'
    };
})
.controller('specialproductsCtrl', ['$scope', 'f', '$rootScope', '$state', '$timeout', ($scope, f, $rootScope, $state, $timeout) => {
    $scope.slickConfig = {};


    $timeout(function () {
        $scope.slickConfig = {
            enabled: true,
            autoplay: true,
            infinite: true,
            draggable: true,
            autoplayspeed: 1000,
            variablewidth: true,
            centermode: true,
            arrows: false,
            method: {},
            event: {
                beforechange: function (event, slick, currentslide, nextslide) {
                },
                afterchange: function (event, slick, currentslide, nextslide) {
                }
            }
        };
        $scope.dataLoaded = true;
    }, 2000);

    $scope.get = (x) => {
        $state.go('product', { title_seo: x.title_seo, sku: x.sku });
    }

    $scope.addToCart = (x) => {
        f.post('Cart', 'AddToCart', { cart: $rootScope.d.cart, x: x }).then((d) => {
            $rootScope.d.cart = d;
            localStorage.cart = JSON.stringify(d);
        });
    }


}])

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

.directive('zoom', function ($window) {

    function link(scope, element, attrs) {

        //SETUP

        var frame, image, zoomlvl, fWidth, fHeight, rect, rootDoc, offsetL, offsetT, xPosition, yPosition, pan;
        //Template has loaded, grab elements.
        scope.$watch('$viewContentLoaded', function () {
            frame = angular.element(document.querySelector("#" + scope.frame))[0];
            image = angular.element(document.querySelector("#" + scope.img))[0];

            zoomlvl = (scope.zoomlvl === undefined) ? "2.5" : scope.zoomlvl
        });



        //MOUSE TRACKER OVER IMG
        scope.trackMouse = function ($event) {

            fWidth = frame.clientWidth;
            fHeight = frame.clientHeight;

            rect = frame.getBoundingClientRect();
            rootDoc = frame.ownerDocument.documentElement;

            //calculate the offset of the frame from the top and left of the document
            offsetT = rect.top + $window.pageYOffset - rootDoc.clientTop
            offsetL = rect.left + $window.pageXOffset - rootDoc.clientLeft

            //calculate current cursor position inside the frame, as a percentage
            xPosition = (($event.pageX - offsetL) / fWidth) * 100
            yPosition = (($event.pageY - offsetT) / fHeight) * 100

            pan = xPosition + "% " + yPosition + "% 0";
            image.style.transformOrigin = pan;

        }

        //MOUSE OVER | ZOOM-IN
        element.on('mouseover', function (event) {
            image.style.transform = 'scale(' + zoomlvl + ')'
        })

        //MOUSE OUT | ZOOM-OUT
        element.on('mouseout', function (event) {
            image.style.transform = 'scale(1)'
        })


    }

    return {
        restrict: 'EA',
        scope: {
            src: '@src',
            frame: '@frame',
            img: '@img',
            zoomlvl: '@zoomlvl'
        },
        template: [
			'<div id="{{ frame }}" class="zoomPanFrame" >',
			'<img id="{{ img }}" class="zoomPanImage" ng-src= "{{ src }}" ng-mousemove="trackMouse($event)"></img>',
			'</div>'
        ].join(''),
        link: link
    };
})
/********** Directives **********/

;