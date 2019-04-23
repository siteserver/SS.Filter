var $api = axios.create({
  baseURL:
    utils.getQueryString("apiUrl") +
    "/" +
    utils.getQueryString("pluginId") +
    "/pages/fields/",
  params: {
    siteId: utils.getQueryInt('siteId')
  },
  withCredentials: true
});

var data = {
  siteId: utils.getQueryInt('siteId'),
  pageLoad: false,
  pageAlert: null,
  pageType: null,
  items: null,
  adminName: null,
  item: null,
  inputTypes: ["SelectOne", "SelectMultiple"],
  tag: {}
};

var methods = {
  apiGetFields: function() {
    var $this = this;

    $api
      .get("")
      .then(function(response) {
        var res = response.data;
        
        $this.items = res.value;
        $this.item = null;
        $this.pageType = "list";
      })
      .catch(function(error) {
        $this.pageAlert = utils.getPageAlert(error);
      })
      .then(function() {
        utils.loading(false);
        $this.pageLoad = true;
      });
  },

  apiGetTagInfo: function(fieldId, tagId) {
    var $this = this;

    utils.loading(true);
    $api
      .get(fieldId + "/" + tagId)
      .then(function(response) {
        var res = response.data;
        $this.tag = res.value;
        utils.openLayer({
          id: "modal",
          title: "设置级联分类",
          width: 550,
          height: 350
        });
      })
      .catch(function(error) {
        $this.pageAlert = utils.getPageAlert(error);
      })
      .then(function() {
        utils.loading(false);
      });
  },

  apiUpdateTagInfo: function() {
    var $this = this;
    utils.closeLayer();
    utils.loading(true);
    $api
      .put(this.tag.fieldId + "/" + this.tag.id, this.tag)
      .then(function(response) {
        var res = response.data;
        $this.pageAlert = {
          type: "success",
          html: "筛选级联分类修改成功！"
        };
        $this.apiGetFields();
      })
      .catch(function(error) {
        $this.pageAlert = utils.getPageAlert(error);
      })
      .then(function() {
        utils.loading(false);
      });
  },

  apiCreateFieldInfo: function(item) {
    var $this = this;

    utils.loading(true);
    $api
      .post("", {
        fieldInfo: item,
        tags: item.tags
      })
      .then(function(response) {
        var res = response.data;
        $this.pageAlert = {
          type: "success",
          html: "筛选分类添加成功！"
        };
        $this.apiGetFields();
      })
      .catch(function(error) {
        $this.pageAlert = utils.getPageAlert(error);
      })
      .then(function() {
        utils.loading(false);
      });
  },

  apiUpdateFieldInfo: function(item) {
    var $this = this;

    utils.loading(true);
    $api
      .put("", {
        fieldInfo: item,
        tags: item.tags
      })
      .then(function(response) {
        var res = response.data;
        $this.pageAlert = {
          type: "success",
          html: "筛选分类修改成功！"
        };
        $this.apiGetFields();
      })
      .catch(function(error) {
        $this.pageAlert = utils.getPageAlert(error);
      })
      .then(function() {
        utils.loading(false);
      });
  },

  apiDeleteField: function(item) {
    var $this = this;

    utils.loading(true);
    $api
      .delete(item.id)
      .then(function(response) {
        var res = response.data;
        $this.pageAlert = {
          type: "success",
          html: "筛选分类删除成功！"
        };
        $this.apiGetFields();
      })
      .catch(function(error) {
        $this.pageAlert = utils.getPageAlert(error);
      })
      .then(function() {
        utils.loading(false);
      });
  },

  btnAddClick: function() {
    this.pageType = "add";
    this.pageAlert = null;
    this.item = {
      id: 0,
      siteId: this.siteId,
      taxis: 0,
      title: "",
      inputType: "SelectOne",
      tags: []
    };
  },

  btnEditClick: function(item) {
    this.pageType = "add";
    this.pageAlert = null;
    this.item = item;
  },

  btnSubmitClick: function() {
    if (this.item.id) {
      this.apiUpdateFieldInfo(this.item);
    } else {
      this.apiCreateFieldInfo(this.item);
    }
  },

  btnCancelClick: function() {
    utils.closeLayer();
    this.pageType = "list";
    this.item = null;
  },

  btnTagClick: function(item, tagId) {
    this.apiGetTagInfo(item.id, tagId);
  },

  btnDeleteClick: function(item) {
    var $this = this;

    utils.alertDelete({
      title: "删除筛选分类",
      text: "此操作将删除筛选分类 " + item.title + "，确定吗？",
      callback: function() {
        $this.apiDeleteField(item);
      }
    });
  },

  btnTagSubmitClick: function() {
    this.apiUpdateTagInfo();
  },

  displayType: function(inputType) {
    if (inputType === "SelectOne") return "单选项";
    else if (inputType === "SelectMultiple") return "多选项";
    else if (inputType === "SelectCascading") return "级联选项";
    return "";
  },

  displayTags: function(item) {
    if (item.tags) {
      return item.tags.join(",");
    }
    return "";
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
  components: {
    "input-tag": InputTag
  },
  created: function() {
    this.apiGetFields();
  }
});
