var $api = axios.create({
  baseURL:
    utils.getQueryString("apiUrl") +
    "/" +
    utils.getQueryString("pluginId") +
    "/pages/check/",
  params: {
    siteId: utils.getQueryInt("siteId"),
    channelId: utils.getQueryInt("channelId"),
    contentId: utils.getQueryInt("contentId")
  },
  withCredentials: true
});

var data = {
  pageLoad: false,
  pageAlert: null,
  fieldInfoList: null,
  isUpdated: false
};

var methods = {
  apiGetFields: function() {
    var $this = this;

    $api
      .get("")
      .then(function(response) {
        var res = response.data;
        $this.fieldInfoList = res.value;
        $this.pageLoad = true;
      })
      .catch(function(error) {
        $this.pageAlert = utils.getPageAlert(error);
      })
      .then(function() {
        $this.pageLoad = true;
        utils.loading(false);
      });
  },

  apiUpdateValue: function(fieldInfo, tagInfo) {
    var $this = this;

    utils.loading(true);
    $api
      .post('', {
        isMultiple: fieldInfo.inputType === "SelectMultiple",
        isAdd: fieldInfo.checkedTagIds.indexOf(tagInfo.id) === -1,
        fieldId: fieldInfo.id,
        tagId: tagInfo.id
      })
      .then(function(response) {
        $this.apiGetFields();
        $this.isUpdated = true;
      })
      .catch(function(error) {
        $this.pageAlert = utils.getPageAlert(error);
      })
      .then(function() {
        utils.loading(false);
      });
  },

  btnTagClick: function(fieldInfo, tagInfo) {
    this.apiUpdateValue(fieldInfo, tagInfo);
  },

  btnCloseClick: function() {
    if (this.isUpdated) {
      parent.location.reload(true);
    }
    utils.closeLayer();
  },

  displayType: function(inputType) {
    if (inputType === "SelectOne") return "单选项";
    else if (inputType === "SelectMultiple") return "多选项";
    else if (inputType === "SelectCascading") return "级联选项";
    return "";
  },

  displayTags: function(fieldInfo) {
    if (fieldInfo.tags) {
      return fieldInfo.tags.join(",");
    }
    return "";
  }
};

var $vue = new Vue({
  el: "#main",
  data: data,
  methods: methods,
  created: function() {
    this.apiGetFields();
  }
});
