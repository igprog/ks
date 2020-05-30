/*!
app.js
(c) 2019-2020 IG PROG, www.igprog.hr
*/
angular.module('admin', ['ngStorage', 'pascalprecht.translate', 'ngMaterial'])
.config(['$httpProvider', '$translateProvider', '$translatePartialLoaderProvider', ($httpProvider, $translateProvider, $translatePartialLoaderProvider) => {

    $translateProvider.useLoader('$translatePartialLoader', {
        urlTemplate: './assets/json/translations/{lang}/{part}.json'
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
                url: './' + service + '.asmx/' + method,
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
        initCodeName: (x) => {
            if (x.id === null) {
                x.code = x.title
                        .replace(/č/g, 'c').replace(/ć/g, 'c').replace(/š/g, 's').replace(/đ/g, 'd').replace(/ž/g, 'z')
                        .replace(/\s/g, "")
                        .substr(0, 16)
                        .toUpperCase();
            }
            return x;
        }
    }
}])

.controller('adminCtrl', ['$scope', '$http', 'f', '$sessionStorage', '$translate', ($scope, $http, f, $sessionStorage, $translate) => {
    var isLogin = $sessionStorage.islogin !== undefined ? $sessionStorage.islogin : false;
    var adminType = $sessionStorage.adminType !== undefined ? $sessionStorage.adminType : 'admin';
    var service = 'Admin';
    var data = {
        admin: {
            userName: null,
            password: null
        },
        isLogin: isLogin,
        adminType: adminType,
        inquiries: null,
        loading: false,
        productGroups: [],
        products: []
    }
    $scope.d = data;

    var getConfig = function () {
        $http.get('../config/config.json')
          .then(function (response) {
              $scope.config = response.data;
          });
    };
    getConfig();

    $scope.toggleTpl = (x) => {
        $scope.tpl = x;
    };

    $scope.f = {
        login: (u) => {
            return login(u);
        },
        logout: () => {
            return logout();
        },
        signup: (u, accept) => {
            return signup(u, accept);
        }
    }

    /********* Login **********/
    var login = (x) => {
        f.post(service, 'Login', { username: x.userName, password: x.password }).then((d) => {
            debugger;
            $scope.d.isLogin = d.isLogin;
            $sessionStorage.islogin = d.isLogin;
            $scope.d.adminType = d.adminType;
            $sessionStorage.adminType = d.adminType;
            if (d.isLogin === true) {
                $scope.toggleTpl('info');
            }
        });
    }

    var logout = () => {
        $scope.d.isLogin = false;
        $sessionStorage.islogin = null;
        $scope.toggleTpl('login');
    };


    if (isLogin) {
        $scope.toggleTpl('info');
    } else {
        $scope.toggleTpl('login');
    }
    /********* Login **********/

}])

.controller('infoCtrl', ['$scope', '$http', 'f', '$mdDialog', ($scope, $http, f, $mdDialog) => {
    var service = 'Info';

    var save = (x) => {
        f.post(service, 'Save', { x: x }).then((d) => {
            $scope.d = d;
        });
    }

    var load = () => {
        f.post(service, 'Load', { lang: null }).then((d) => {
            $scope.d = d;
        });
    }
    load();

    var openTranPopup = function (x, type) {
        $mdDialog.show({
            controller: tranPopupCtrl,
            templateUrl: './assets/partials/popup/tran.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { data: x, type: type }
        })
       .then(function (x) {
       }, function () {
       });
    }

    var tranPopupCtrl = function ($scope, $mdDialog, $http, d, f) {
        var service = 'Tran';
        var init = () => {
            f.post(service, 'Init', {}).then((res) => {
                $scope.d = {
                    tran: res,
                    data: d.data,
                    langs: [
                        {
                            id: null,
                            lang: 'en',
                            tran: null
                        },
                        {
                            id: null,
                            lang: 'ru',
                            tran: null
                        }
                    ]
                }
                $scope.d.tran.productId = null;
                $scope.d.tran.recordType = d.type;
                angular.forEach($scope.d.langs, function (value, key) {
                    f.post(service, 'Get', { productId: null, recordType: d.type, lang: value.lang }).then((res) => {
                        if (res.length > 0) {
                            $scope.d.langs[key].id = res[0].id;
                            $scope.d.langs[key].tran = res[0].tran;
                        }
                    });
                });
            });
        }
        init();

        var save = (d, x) => {
            $scope.d.tran.id = x.id;
            $scope.d.tran.tran = x.tran;
            $scope.d.tran.lang = x.lang;
            //console.log(d.tran);

            f.post(service, 'Save', { x: d.tran }).then((d) => {
                init();
            });

            //$mdDialog.hide();
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (d, x) {
            save(d, x);
        }
    };

    $scope.f = {
        save: (x) => {
            return save(x)
        },
        upload: (x) => {
            return upload(x);
        },
        openTranPopup: (x, type) => {
            return openTranPopup(x, type)
        }
    }

}])

.controller('productGroupsCtrl', ['$scope', '$http', 'f', '$sessionStorage', ($scope, $http, f, $sessionStorage) => {
    var service = 'ProductGroups';
    var adminType = $sessionStorage.adminType !== undefined ? $sessionStorage.adminType : 'admin';

        var data = {
            loading: false,
            records: [],
            adminType: adminType
        }
        $scope.d = data;

        var init = () => {
            f.post(service, 'Init', {}).then((d) => {
                $scope.d.records.push(d);
            });
        }

        var load = () => {
            $scope.d.loading = true;
            f.post(service, 'Load', {}).then((d) => {
                $scope.d.records = d;
                $scope.d.loading = false;
            });
        }
        load();

        var save = (x) => {
            f.post(service, 'Save', { x: x }).then((d) => {
                //$scope.d.records = d;
                alert(d);
            });
        }

        var remove = (x) => {
            if (confirm('Briši grupu?')) {
                f.post(service, 'Delete', { x: x }).then((d) => {
                    $scope.d.records = d;
                });
            }
        }

        $scope.f = {
            init: () => {
                return init();
            },
            save: (x) => {
                x.parent = x.code;  // only for main product group
                return save(x);
            },
            saveSubGroup: (x) => {
                return save(x);
            },
            get: () => {
                return get();
            },
            remove: (x) => {
                return remove(x)
            },
            initSubGroup: (x) => {
                debugger;
                f.post(service, 'Init', {}).then((d) => {
                    d.parent = x.code;
                    x.subGroups.push(d);
                });
            },
            initCodeName: (x) => {
                x = f.initCodeName(x);
            }
        }
}])

.controller('brandsCtrl', ['$scope', '$http', 'f', '$sessionStorage', ($scope, $http, f, $sessionStorage) => {
    var service = 'Brands';
    var adminType = $sessionStorage.adminType !== undefined ? $sessionStorage.adminType : 'admin';

    var data = {
        loading: false,
        records: [],
        adminType: adminType
    }
    $scope.d = data;

    var init = () => {
        f.post(service, 'Init', {}).then((d) => {
            $scope.d.records.push(d);
        });
    }

    var load = () => {
        $scope.d.loading = true;
        f.post(service, 'Load', {}).then((d) => {
            $scope.d.records = d;
            $scope.d.loading = false;
        });
    }
    load();

    var save = (x) => {
        f.post(service, 'Save', { x: x }).then((d) => {
            $scope.d.records = d;
        });
    }

    var remove = (x) => {
        if (confirm('Briši brend?')) {
            f.post(service, 'Delete', { x: x }).then((d) => {
                $scope.d.records = d;
            });
        }
    }

    $scope.f = {
        init: () => {
            return init();
        },
        save: (x) => {
            return save(x);
        },
        get: () => {
            return get();
        },
        remove: (x) => {
            return remove(x)
        },
        initCodeName: (x) => {
            x = f.initCodeName(x);
        }
    }
}])

.controller('productsCtrl', ['$scope', '$http', 'f', '$mdDialog', ($scope, $http, f, $mdDialog) => {
    var service = 'Products';
    var data = {
        loading: false,
        productGroups: [],
        brands: [],
        records: [],
        currProduct: null,
        currProductGroup: null,
        productGroupId: null,
        search: null,
        responseTime: 0
    }
    $scope.d = data;

    var load = (productGroup, search) => {
        var pg_code = null;
        if (productGroup === null) {
            $scope.d.currProductGroup = null;
        } else {
            pg_code = productGroup.code;
            $scope.d.currProductGroup = productGroup;
        }
        $scope.d.currProductGroup = productGroup;
        $scope.d.loading = true;
        f.post(service, 'Load', { lang: 'hr', productGroup: pg_code, brand: null, search: search }).then((d) => {
            $scope.d.records = d.data;
            $scope.d.responseTime = d.responseTime;
            $scope.d.loading = false;
        });
    }

    var loadProductGroups = () => {
        //$scope.d.loading = true;
        f.post('ProductGroups', 'Load', {}).then((d) => {
            $scope.d.productGroups = d;
            //load($scope.d.productGroups[0]);
            load(null, null);
            $scope.d.productGroupId = $scope.d.productGroups[0].id;
            //$scope.d.loading = false;

        });
    }
    loadProductGroups();

    var loadBrands = () => {
        //$scope.d.loading = true;
        f.post('Brands', 'Load', {}).then((d) => {
            $scope.d.brands = d;
            //$scope.d.loading = false;

        });
    }
    loadBrands();

    var save = (x, idx) => {
        debugger;
        if (x.sku === null) {
            alert('Upiši SKU (kataloški broj)');
            return false;
        }
        if (x.style === null) { x.style = x.sku; }
        if (x.discount.perc === null) { x.discount.perc = 0; }
        f.post(service, 'Save', { x: x }).then((d) => {
            debugger;
            $scope.d.records[idx].id = d;
        });
    }

    var upload = (x, idx) => {
        var content = new FormData(document.getElementById('formUpload_' + x.id));
        $http({
            url: '../UploadHandler.ashx',
            method: 'POST',
            headers: { 'Content-Type': undefined },
            data: content,
        }).then(function (response) {
            loadProductGallery(x);

        },
        function (response) {
            alert(response.data.d);
        });
    }

    var loadProductGallery = (x) => {
        debugger;
        f.post(service, 'LoadProductGallery', { productId: x.id }).then((d) => {
            x.gallery = d;
            if (x.gallery.length === 1) {
                setMainImg(x, x.gallery[0]);
            }
        });
    }

    var deleteImg = (x, img) => {
        if (confirm('Briši sliku?')) {
            f.post(service, 'DeleteImg', { x: x, img: img }).then((d) => {
                //$scope.d.records = d;
                //$scope.d.records[idx] = d;
                loadProductGallery(x);
            });
        }
    }

    var newProduct = () => {
        f.post(service, 'Init', {}).then((d) => {
            $scope.d.records.push(d);
        });
    }

    var remove = (x) => {
        if (confirm('Briši proizvod ' + x.title + ' ?')) {
            f.post(service, 'Delete', { x: x }).then((d) => {
                $scope.d.records = d.data;
            });
        }
    }

    var setMainImg = (x, img) => {
        f.post(service, 'SetMainImg', { x: x, img: img }).then((d) => {
            //$scope.d.records = d;
        });
    }

    var openTranPopup = function (x, type) {
        $mdDialog.show({
            controller: tranPopupCtrl,
            templateUrl: './assets/partials/popup/tran.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { data: x, type: type }
        })
       .then(function (x) {
       }, function () {
       });
    }

    var tranPopupCtrl = function ($scope, $mdDialog, $http, d, f) {
        var service = 'Tran';
        var init = () => {
            f.post(service, 'Init', {}).then((res) => {
                $scope.d = {
                    tran: res,
                    data: d.data,
                    langs: [
                        {
                            id: null,
                            lang: 'en',
                            tran: null
                        },
                        {
                            id: null,
                            lang: 'ru',
                            tran: null
                        }
                    ]
                }
                $scope.d.tran.productId = d.data.id;
                $scope.d.tran.recordType = d.type;

                angular.forEach($scope.d.langs, function (value, key) {
                    f.post(service, 'Get', { productId: d.data.id, recordType: d.type, lang: value.lang }).then((res) => {
                        if (res.length > 0) {
                            $scope.d.langs[key].id = res[0].id;
                            $scope.d.langs[key].tran = res[0].tran;
                        }
                    });
                });
            });
        }
        init();

        var save = (d, x) => {
            d.tran.id = x.id;
            d.tran.tran = x.tran;
            d.tran.lang = x.lang;
            console.log(d.tran);


            f.post(service, 'Save', { x: d.tran }).then((d) => {
                init();
            });

            //$mdDialog.hide();
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (d, x) {
            save(d, x);
        }
    };

    var setProductGroup = (x, pg) => {
        x.productGroup = pg;
    }

    $scope.f = {
        load: (productGroup, search) => {
            debugger;
            return load(productGroup, search);
        },
        save: (x, idx) => {
            return save(x, idx)
        },
        upload: (x, idx) => {
            return upload(x, idx);
        },
        deleteImg: (x, img) => {
            return deleteImg(x, img);
        },
        newProduct: () => {
            return newProduct();
        },
        remove: (x) => {
            return remove(x);
        },
        setMainImg: (x, img) => {
            return setMainImg(x, img);
        },
        openTranPopup: (x, type) => {
            return openTranPopup(x, type);
        },
        setProductGroup: (x, pg) => {
            return setProductGroup(x, pg);
        }
    }
}])

.controller('reviewsCtrl', ['$scope', '$http', 'f', '$mdDialog', ($scope, $http, f, $mdDialog) => {
        var service = 'Review';
        var data = {
            loading: false,
            records: []
        }
        $scope.d = data;

        var load = () => {
            $scope.d.loading = true;
            f.post(service, 'Load', { }).then((d) => {
                $scope.d.records = d;
                $scope.d.loading = false;
            });
        }
        load();

        var save = (x) => {
            debugger;
            f.post(service, 'Save', { x: x }).then((d) => {
                //$scope.d.records = d;
            });
        }

        var remove = (x) => {
            if (confirm('Briši recenziju?')) {
                f.post(service, 'Delete', { x: x }).then((d) => {
                    //$scope.d.records = d;
                });
            }
        }

        $scope.f = {
            load: () => {
                return load();
            },
            save: (x) => {
                return save(x)
            },
            remove: (x) => {
                return remove(x);
            }
        }
    }])

.controller('featuresCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
        var service = 'Features';
        var data = {
            loading: false,
            records: []
        }
        $scope.d = data;

        var add = () => {
            $scope.d.records.push({});
        }

        var save = (x) => {
            f.post(service, 'Save', { x: x }).then((d) => {
                $scope.d.records = d;
            });
        }

        var load = (type) => {
            $scope.d.loading = true;
            f.post(service, 'Load', { type: type }).then((d) => {
                $scope.d.records = d;
                $scope.d.loading = false;
            });
        }
        load(null);

        var remove = (x, idx) => {
            if (confirm('Briši značajku?')) {
                x.splice(idx, 1);
            }
        }

        $scope.f = {
            add: () => {
                return add();
            },
            save: (x) => {
                return save(x)
            },
            remove: (x, idx) => {
                return remove(x, idx)
            }
        }

    }])

.controller('uploadCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Info';

    var upload = (x) => {
        var content = new FormData(document.getElementById('formUpload_' + x));
        $http({
            url: '../UploadHandler.ashx',
            method: 'POST',
            headers: { 'Content-Type': undefined },
            data: content,
        }).then(function (response) {
            location.reload(true);
        },
        function (response) {
            alert(response.data.d);
        });
    }

    var removeMainImg = (x) => {
        if (confirm('Briši proizvod?')) {
            f.post(service, 'DeleteMainImg', { img: x }).then((d) => {
                location.reload(true);
            });
        }
    }

    $scope.tick = 0;
    var getTime = () => {
        var d = new Date();
        $scope.tick = d.getTime();
    }
    getTime();

    $scope.f = {
        upload: (x) => {
            return upload(x);
        },
        removeMainImg: (x) => {
            return removeMainImg(x);
        }
    }

}])

.controller('countriesCtrl', ['$scope', '$http', 'f', function ($scope, $http, f) {
    var service = 'Orders';
    $scope.d = null;
    var load = function () {
        f.post(service, 'GetCountries', {}).then((d) => {
            $scope.d = d;
        });
    }
    load();

    $scope.add = function (x) {
        x.push({});
    }

    $scope.remove = function (x, idx) {
        x.splice(idx, 1);

    }

    $scope.save = function (x) {
        f.post(service, 'SaveCountries', { x: x }).then((d) => {
            $scope.d = d;
        });
    }

}])

.controller('orderOptionsCtrl', ['$scope', '$http', 'f', function ($scope, $http, f) {
    var service = 'Orders';
    $scope.d = null;
    var load = function () {
        f.post(service, 'GetOrderOptions', {}).then((d) => {
            $scope.d = d;
        });
    }
    load();

    $scope.add = function (x) {
        x.push({});
    }

    $scope.remove = function (x, idx) {
        x.splice(idx, 1);

    }

    $scope.save = function (x) {
        f.post(service, 'SaveOrderOptions', { x: x }).then((d) => {
            $scope.d = d;
        });
    }

}])

/********** Directives **********/
//.directive('reservationDirective', () => {
//    return {
//        restrict: 'E',
//        scope: {
//            service: '='
//        },
//        templateUrl: './assets/partials/reservation.html'
//    };
//})

.directive('detailsDirective', () => {
    return {
        restrict: 'E',
        scope: {
            service: '=',
            desc: '=',
            img: '=',
            price: '='
        },
        templateUrl: './assets/partials/directive/details.html'
    };
})

.directive('navbarDirective', () => {
    return {
        restrict: 'E',
        scope: {
            site: '='
        },
        templateUrl: './assets/partials/directive/navbar.html'
    };
})

.directive('cardDirective', () => {
    return {
        restrict: 'E',
        scope: {
            service: '=',
            desc: '=',
            link: '='
        },
        templateUrl: './assets/partials/directive/card.html'
    };
})

.directive('uploadDirective', () => {
    return {
        restrict: 'E',
        scope: {
            id: '=',
            img: '='
        },
        templateUrl: './assets/partials/directive/upload.html',
        controller: 'uploadCtrl'
    };
})

.directive('loadingDirective', () => {
    return {
        restrict: 'E',
        scope: {
            btntitle: '=',
            loadingtitle: '=',
            value: '=',
            pdf: '=',
            size: '='
        },
        templateUrl: './assets/partials/directive/loading.html'
    };
})

.directive('jsonDirective', () => {
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
.controller('jsonCtrl', ['$scope', ($scope) => {
    $scope.isShow = false;
    $scope.show = () => {
        $scope.isShow = !$scope.isShow;
    }
}])

//.directive('modalDirective', () => {
//    return {
//        restrict: 'E',
//        scope: {
//            id: '=',
//            headertitle: '=',
//            data: '=',
//            src: '='
//        },
//        templateUrl: './assets/partials/modal.html'
//    };
//})

.directive('tranBtn', () => {
    return {
        restrict: 'E',
        scope: {
        },
        templateUrl: './assets/partials/directive/tranbtn.html'
    };
})

.directive('allowOnlyNumbers', function () {
    return {
        restrict: 'A',
        link: function (scope, elm, attrs, ctrl) {
            elm.on('keydown', function (event) {
                var $input = $(this);
                var value = $input.val();
                value = value.replace(',', '.');
                $input.val(value);
                if (event.which === 64 || event.which === 16) {
                    return false;
                } else if (event.which >= 48 && event.which <= 57) {
                    return true;
                } else if (event.which >= 96 && event.which <= 105) {
                    return true;
                } else if ([8, 13, 27, 37, 38, 39, 40].indexOf(event.which) > -1) {
                    return true;
                } else if (event.which === 110 || event.which === 188 || event.which === 190) {
                    return true;
                } else if (event.which === 46) {
                    return true;
                } else {
                    event.preventDefault();
                    return false;
                }
            });
        }
    }
})
/********** Directives **********/


;