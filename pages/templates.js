var $api = axios.create({
  baseURL:
    utils.getQueryString("apiUrl") +
    "/" +
    utils.getQueryString("pluginId") +
    "/pages/templates/",
  params: {
    siteId: utils.getQueryInt('siteId')
  },
  withCredentials: true
});

var data = {
  siteId: utils.getQueryString('siteId'),
  apiUrl: utils.getQueryString('apiUrl'),
  pageLoad: false,
  pageConfig: null,
  pageAlert: {
    type: 'primary',
    html: '表单标签：<mark>&lt;stl:filter type="模板文件夹"&gt;&lt;/stl:filter&gt;</mark>，如果希望自定义模板样式，可以点击克隆按钮然后修改模板代码。'
  },
  pageType: '',
  templateInfoList: null,
  name: null,
  templateHtml: null,
};

var methods = {
  getIconUrl: function (templateInfo) {
    return '../templates/' + templateInfo.name + '/' + templateInfo.icon;
  },

  loadTemplates: function () {
    var $this = this;

    if (this.pageLoad) {
      utils.loading(true);
    }

    $api.get('').then(function (response) {
      var res = response.data;

      $this.templateInfoList = res.value;
      $this.pageType = 'list';
    }).catch(function (error) {
      $this.pageAlert = utils.getPageAlert(error);
    }).then(function () {
      utils.loading(false);
      $this.pageLoad = true;
    });
  },

  getPreviewUrl: function (templateInfo) {
    return '../templates/' + templateInfo.name + '/' + templateInfo.main + '?siteId=' + this.siteId + '&apiUrl=' + encodeURIComponent(this.apiUrl)
  },

  btnEditClick: function (name) {
    utils.openLayer({
      title: '模板设置',
      url: utils.getPageUrl('templatesLayerEdit.html') + '&name=' + name
    });
  },

  btnDeleteClick: function (template) {
    var $this = this;
    utils.alertDelete({
      title: '删除模板',
      text: '此操作将删除模板' + template.name + '，确认吗？',
      callback: function () {
        utils.loading(true);
        $api.delete('', {
          params: {
            name: template.name
          }
        }).then(function (response) {
          var res = response.data;

          $this.templateInfoList = res.value;
          $this.pageType = 'list';
        }).catch(function (error) {
          $this.pageAlert = utils.getPageAlert(error);
        }).then(function () {
          utils.loading(false);
        });
      }
    });
  },

  btnHtmlClick: function (templateInfo) {
    utils.loading(true);
    var url = utils.getPageUrl('templateHtml.html') + '&name=' + templateInfo.name;
    location.href = url;
  },

  btnSubmitClick: function () {
    var $this = this;
    utils.loading(true);
    $api.post('', {
      name: this.name,
      templateHtml: this.templateHtml
    }).then(function (response) {
      var res = response.data;

      swal({
        toast: true,
        type: 'success',
        title: "模板编辑成功！",
        showConfirmButton: false,
        timer: 2000
      }).then(function () {
        $this.pageType = 'list';
      });
    }).catch(function (error) {
      $this.pageAlert = utils.getPageAlert(error);
    }).then(function () {
      utils.loading(false);
    });
  },

  btnNavClick: function(pageName) {
    utils.loading(true);
    var url = utils.getPageUrl(pageName);
    location.href = url;
  }
};

var $vue = new Vue({
  el: "#main",
  data: data,
  methods: methods,
  created: function () {
    this.loadTemplates();
  }
});