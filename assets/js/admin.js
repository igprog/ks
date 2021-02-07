/*!
app.js
(c) 2019-2021 IG PROG, www.igprog.hr
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
        debugger;
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

    //var data = {
    //    loading: false,
    //    record: null,
    //    productGroups: [],
    //}
    //$scope.d = data;


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

    var loadProductGroups = () => {
        f.post('ProductGroups', 'Load', {}).then((d) => {
            $scope.productGroups = d;
        });
    }
    loadProductGroups();

    var setSepcialProductGroup = (x) => {
        debugger;
        $scope.d.specialProductGroup = x.code;
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
        },
        setSepcialProductGroup: (x) => {
            return setSepcialProductGroup(x);
        }
    }

}])

.controller('bannersCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Banners';

    var data = {
        loading: false,
        records: null,
        imgFolder: 'banners'
    }
    $scope.d = data;

    var init = () => {
        f.post(service, 'Init', {}).then((d) => {
            $scope.d.records.push(d);
        });
    }

    var save = (x) => {
        f.post(service, 'Save', { x: x }).then((d) => {
            $scope.d.records = d;
        });
    }

    var load = () => {
        f.post(service, 'Load', { isactive: false }).then((d) => {
            $scope.d.records = d;
        });
    }
    load();

    var remove = (x) => {
        if (confirm('Briši?')) {
            f.post(service, 'Delete', { x: x }).then((d) => {
                $scope.d.records = d;
            });
        }
    }

    var upload = (x, idx) => {
        var content = new FormData(document.getElementById('formUpload_' + idx));
        $http({
            url: '../UploadHandler.ashx',
            method: 'POST',
            headers: { 'Content-Type': undefined },
            data: content,
        }).then(function (response) {
            x.img = response.data;
            save(x, idx)
        },
        function (response) {
            alert(response.data.d);
        });
    }

    $scope.f = {
        init: () => {
            return init();
        },
        save: (x) => {
            return save(x);
        },
        remove: (x) => {
            return remove(x)
        },
        upload: (x, idx) => {
            return upload(x, idx);
        }
    }

}])

.controller('productGroupsCtrl', ['$scope', '$http', 'f', '$sessionStorage', ($scope, $http, f, $sessionStorage) => {
    var service = 'ProductGroups';
    var adminType = $sessionStorage.adminType !== undefined ? $sessionStorage.adminType : 'admin';

        var data = {
            loading: false,
            records: [],
            adminType: adminType,
            imgFolder: 'productgroups'
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
                angular.forEach(d, function (val, key) {
                    debugger;
                    if (val.discount.from !== null) {
                        $scope.d.records[key].discount.from = new Date(val.discount.from);
                    }
                    if (val.discount.to !== null) {
                        $scope.d.records[key].discount.to = new Date(val.discount.to);
                    }
                    /***** SubGroups Date *****/
                    angular.forEach(val.subGroups, function (val1, key1) {
                        debugger;
                        if (val1.discount.from !== null) {
                            $scope.d.records[key].subGroups[key1].discount.from = new Date(val1.discount.from);
                        }
                        if (val1.discount.to !== null) {
                            $scope.d.records[key].subGroups[key1].discount.to = new Date(val1.discount.to);
                        }
                    });
                });
                $scope.d.loading = false;
            });
        }
        load();

        var save = (x, idx) => {
            debugger;
            var data = angular.copy(x);
            if (x.discount.from !== null) {
                data.discount.from = f.setDate(x.discount.from);
            }
            if (x.discount.to !== null) {
                data.discount.to = f.setDate(x.discount.to);
            }
            f.post(service, 'Save', { x: data }).then((d) => {
                debugger;
                //$scope.d.records[idx] = d;
                x = d;
                load();
                //alert(d);
            });
        }

        var remove = (x) => {
            if (confirm('Briši grupu?')) {
                f.post(service, 'Delete', { x: x }).then((d) => {
                    $scope.d.records = d;
                });
            }
        }

        //var loadImg = (x) => {
        //    f.post(service, 'LoadImg', { x: x }).then((d) => {
        //        //$scope.d.records = d;
        //    });
        //}

        var upload = (x, idx) => {
            //delete previous image from folder
            debugger;
            var content = new FormData(document.getElementById('formUpload_' + x.id));
            $http({
                url: '../UploadHandler.ashx',
                method: 'POST',
                headers: { 'Content-Type': undefined },
                data: content,
            }).then(function (response) {
                debugger;
                //$scope.d.records[idx].img = response.data;
                x.img = response.data;
                save(x, idx)
                //TODO save productImg to bd
            },
            function (response) {
                alert(response.data.d);
            });
        }

        $scope.f = {
            init: () => {
                return init();
            },
            save: (x, idx) => {
                x.parent.code = x.code;  // only for main product group
                return save(x, idx);
            },
            saveSubGroup: (x) => {
                return save(x);
            },
            remove: (x) => {
                return remove(x)
            },
            initSubGroup: (x) => {
                debugger;
                f.post(service, 'Init', {}).then((d) => {
                    d.parent.code = x.code;
                    x.subGroups.push(d);
                });
            },
            initCodeName: (x) => {
                x = f.initCodeName(x);
            },
            upload: (x, idx) => {
                return upload(x, idx)
            },
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

.controller('productsCtrl', ['$scope', '$http', 'f', '$sessionStorage', '$mdDialog', ($scope, $http, f, $sessionStorage, $mdDialog) => {
    var service = 'Products';
    var adminType = $sessionStorage.adminType !== undefined ? $sessionStorage.adminType : 'admin';
    var defaultLimit = 20;

    var data = {
        loading: false,
        productGroups: [],
        brands: [],
        records: [],
        adminType: adminType,
        currProduct: null,
        currProductGroup: null,
        productGroupId: null,
        search: null,
        responseTime: 0,
        dataSheetFolder: 'datasheet',
        accessories: [],
        fireboxInserts: [
            {
                code: 1,
                title: 'bioethanol'
            },
            {
                code: 2,
                title: 'electric'
            }
        ],
        limit: defaultLimit
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
        f.post(service, 'Load', { lang: 'hr', productGroup: pg_code, brand: null, search: search, type: null, isDistinctStyle: false, limit: data.limit }).then((d) => {
            $scope.d.records = d.data;
            $scope.d.responseTime = d.responseTime;
            $scope.d.loading = false;
            data.limit = defaultLimit;
        });
    }

    var loadAll = () => {
        data.limit = null;
        var productGroup = $scope.d.productGroup !== undefined ? $scope.d.productGroup : null;
        var search = $scope.d.search !== undefined ? $scope.d.search : null;
        load($scope.d.currProductGroup, search);
    }

    var loadProductGroups = () => {
        f.post('ProductGroups', 'Load', {}).then((d) => {
            $scope.d.productGroups = d;
            load(null, null);
            $scope.d.productGroupId = $scope.d.productGroups[0].id;
        });
    }
    loadProductGroups();

    var loadBrands = () => {
        f.post('Brands', 'Load', {}).then((d) => {
            $scope.d.brands = d;
        });
    }
    loadBrands();

    var loadAccessories = () => {
        f.post(service, 'Load', { lang: 'hr', productGroup: 'accessories', brand: null, search: null, type: null, isDistinctStyle: false, limit: data.limit }).then((d) => {
            $scope.d.accessories = d;
        });
    }
    loadAccessories();

    var save = (x, idx) => {
        if (x.sku === null) {
            alert('Upiši SKU (kataloški broj)');
            return false;
        }
        if (x.style === null) { x.style = x.sku; }
        if (x.discount.perc === null) { x.discount.perc = 0; }
        var data = angular.copy(x);
        if (data.discount.from !== null) {
            data.discount.from = f.setDate(data.discount.from);
        }
        if (data.discount.to !== null) {
            data.discount.to = f.setDate(data.discount.to);
        }
        f.post(service, 'Save', { x: data }).then((d) => {
            if (x.id === null) {
                x.id = d;
            }
            //$scope.d.records[idx].id = d;
        });
    }

    var get = (sku, idx) => {
        if (sku !== null) {
            f.post(service, 'Get', { sku: sku, lang: 'hr' }).then((d) => {
                $scope.d.records[idx] = d;
                $scope.d.records[idx].discount.from = new Date(d.discount.from);
                $scope.d.records[idx].discount.to = new Date(d.discount.to);
            });
        }
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

    var uploadDataSheet = (x, idx) => {
        var content = new FormData(document.getElementById('formUploadDataSheet_' + x.id));
        $http({
            url: '../UploadHandler.ashx',
            method: 'POST',
            headers: { 'Content-Type': undefined },
            data: content,
        }).then(function (response) {
            debugger;
            x.dataSheet.push(response.data);
            //x.dataSheet[] = response.data;
            save(x, idx);
            //loadProductGallery(x);
        },
        function (response) {
            alert(response.data.d);
        });
    }

    var loadProductGallery = (x) => {
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
            $scope.d.loading = true;
            f.post(service, 'Delete', { x: x }).then((d) => {
                $scope.d.records = d.data;
                $scope.d.loading = false;
            });
        }
    }

    var setMainImg = (x, img) => {
        f.post(service, 'SetMainImg', { x: x, img: img }).then((d) => {
            x = d;
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

    var setProductGroup = (x, pg, sg) => {
        if (x.id === null) {
            x.productGroup = sg;
            f.post('Features', 'getProductFeatures', { productGroup: pg }).then((d) => {
                x.features = d;
            });
        }
    }

    var addRelated = (x) => {
        f.post(service, 'Init', {}).then((d) => {
            x.relatedProducts.push(d);
        });
    }

    var removeRelated = (x, idx) => {
        if (confirm('Briši?')) {
            x.splice(idx, 1);
        }
    }

    var addKeyFeatures = (x) => {
        x.push({code: null, title: null});
    }

    var removeKeyFeatures = (x, idx) => {
        if (confirm('Briši?')) {
            x.splice(idx, 1);
        }
    }

    var deleteDataSheet = (x, file, idx) => {
        if (confirm('Briši?')) {
            x.dataSheet.splice(idx, 1);
            f.post(service, 'DeleteDataSheet', { x: x, file: file }).then((d) => {
            });
        }
    }

    var importProductsCsv = () => {
        f.post(service, 'ImportProductsCsv', {}).then((d) => {
            alert(d);
        });
    }

    $scope.f = {
        load: (productGroup, search) => {
            return load(productGroup, search);
        },
        loadAll: () => {
            return loadAll();
        },
        save: (x, idx) => {
            return save(x, idx)
        },
        get: (sku, idx) => {
            return get(sku, idx)
        },
        upload: (x, idx) => {
            return upload(x, idx);
        },
        uploadDataSheet: (x, idx) => {
            return uploadDataSheet(x, idx);
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
        setProductGroup: (x, pg, sg) => {
            return setProductGroup(x, pg, sg);
        },
        addRelated: (x) => {
            return addRelated(x);
        },
        removeRelated: (x, idx) => {
            return removeRelated(x, idx);
        },
        addKeyFeatures: (x) => {
            return addKeyFeatures(x);
        },
        removeKeyFeatures: (x, idx) => {
            return removeKeyFeatures(x, idx);
        },
        deleteDataSheet: (x, file, idx) => {
            return deleteDataSheet(x, file, idx);
        },
        importProductsCsv: () => {
            return importProductsCsv();
        },
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

.controller('ordersCtrl', ['$scope', '$http', 'f', '$mdDialog', ($scope, $http, f, $mdDialog) => {
    var service = 'Orders';
    var data = {
        loading: false,
        records: []
    }
    $scope.d = data;

    var load = () => {
        $scope.d.loading = true;
        f.post(service, 'Load', {}).then((d) => {
            $scope.d.records = d;
            $scope.d.loading = false;
        });
    }
    load();

    var getUser = (id, idx) => {
        debugger;
        f.post('Users', 'Get', { id: id }).then((d) => {
            $scope.d.records[idx].user = d;
        });
    }

    var save = (x) => {
        debugger;
        f.post(service, 'Save', { x: x }).then((d) => {
            //$scope.d.records = d;
        });
    }

    var remove = (x) => {
        if (confirm('Briši narudžbu?')) {
            f.post(service, 'Delete', { x: x }).then((d) => {
                //$scope.d.records = d;
            });
        }
    }

    $scope.f = {
        load: () => {
            return load();
        },
        getUser: (id, idx) => {
            return getUser(id, idx);
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
        records: [],
        productGroups: []
    }
    $scope.d = data;

    var add = (x) => {
        f.post(service, 'Init', { x: x }).then((d) => {
            x.push(d);
        });
        
        //$scope.d.records.push({});
    }

    var save = (x) => {
        f.post(service, 'Save', { x: x }).then((d) => {
            $scope.d.records = d;
        });
    }

    var load = (type) => {
        $scope.d.loading = true;
        f.post(service, 'Load', { type: type}).then((d) => {
            $scope.d.records = d;
            $scope.d.loading = false;
        });
    }
    load(null);

    var remove = (x, idx) => {
        if (confirm('Briši?')) {
            x.splice(idx, 1);
        }
    }

    var addProductGroup = (x) => {
        x.push({});
    }

    //var removeProductGroup = (x) => {
    //    x.push({});
    //}

    var loadProductGroups = () => {
        $scope.d.loading = true;
        f.post('ProductGroups', 'Load', {}).then((d) => {
            $scope.d.productGroups = d;
            $scope.d.loading = false;
        });
    }
    loadProductGroups();

    $scope.f = {
        add: (x) => {
            return add(x);
        },
        save: (x) => {
            return save(x)
        },
        remove: (x, idx) => {
            return remove(x, idx)
        },
        addProductGroup: (x) => {
            return addProductGroup(x);
        }
    }

}])

.controller('colorsCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
        var service = 'Colors';
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

        var load = () => {
            $scope.d.loading = true;
            f.post(service, 'Load', {}).then((d) => {
                $scope.d.records = d;
                $scope.d.loading = false;
            });
        }
        load();

        var remove = (x, idx) => {
            if (confirm('Briši?')) {
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

.controller('subscribersCtrl', ['$scope', '$http', 'f', '$mdDialog', ($scope, $http, f, $mdDialog) => {
        var service = 'Subscribe';
        var data = {
            loading: false,
            records: []
        }
        $scope.d = data;

        var load = () => {
            $scope.d.loading = true;
            f.post(service, 'Load', {}).then((d) => {
                $scope.d.records = d;
                $scope.d.loading = false;
            });
        }
        load();

        var remove = (x) => {
            if (confirm('Briši?')) {
                f.post(service, 'Delete', { x: x }).then((d) => {
                    $scope.d.records = d;
                });
            }
        }

        $scope.f = {
            load: () => {
                return load();
            },
            remove: (x) => {
                return remove(x);
            }
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
            loadingtitle: '=',
            val: '=',
            size: '='
        },
        templateUrl: './assets/partials/directive/loading.html'
    };
})

//.directive('loadingDirective', () => {
//    return {
//        restrict: 'E',
//        scope: {
//            btntitle: '=',
//            loadingtitle: '=',
//            value: '=',
//            pdf: '=',
//            size: '='
//        },
//        templateUrl: './assets/partials/directive/loading.html'
//    };
//})

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