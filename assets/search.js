var axiosParams = {
  baseURL:
    utils.getQueryString("apiUrl") +
    "/ss.filter/search/",
  params: {
    siteId: utils.getQueryInt('siteId')
  },
  withCredentials: true
};
if (utils.getQueryInt('channelId')) {
  axiosParams.params.channelId = utils.getQueryInt('channelId')
}
var $api = axios.create(axiosParams);

var data = {
  siteId: utils.getQueryInt('siteId'),
  pageLoad: false,
  pageAlert: null,
  fields: null,
  results: null,
  top: 20,
  count: 0,
  pageCount: 0,
  page: 0
};

var methods = {
  getFields: function () {
    var $this = this;

    $api.get('')
      .then(function (response) {
        var res = response.data;

        $this.fields = res.value;
      })
      .catch(function(error) {
        $this.pageAlert = utils.getPageAlert(error);
      })
      .then(function() {
        utils.loading(false);
        $this.pageLoad = true;
      });
  },

  apiGetValues: function (page) {
    var $this = this;

    utils.loading(true);

    var params = {
      top: this.top,
      skip: this.top * (page - 1)
    };
    for (var index = 0; index < this.fields.length; index++) {
      var field = this.fields[index];
      params[field.id] = field.checkedTagIds;
    }

    $api.get('values', { params : params })
      .then(function (response) {
        var res = response.data;

        $this.results = res.value;
        $this.count = res.count;
        $this.page = page;
        $this.pageCount = Math.ceil($this.count / $this.top);
      })
      .catch(function(error) {
        $this.pageAlert = utils.getPageAlert(error);
      })
      .then(function() {
        utils.loading(false);
      });
  },

  btnTagClick: function (field, tagInfo) {
    if (field.checkedTagIds.indexOf(tagInfo.id) === -1) {
      if (field.inputType === 'SelectOne') {
        for (var i = 0; i < field.tagInfoList.length; i++) {
          if (field.checkedTagIds.indexOf(field.tagInfoList[i].id) !== -1) {
            field.checkedTagIds.splice(field.checkedTagIds.indexOf(field.tagInfoList[i].id), 1);
          }
        }
      }
      field.checkedTagIds.push(tagInfo.id);
    } else {
      field.checkedTagIds.splice(field.checkedTagIds.indexOf(tagInfo.id), 1);
    }
    this.apiGetValues(1);
  },

  btnFirstClick: function () {
    if (this.page <= 1) return;
    this.apiGetValues(1);
  },

  btnPreviousClick: function () {
    if (this.page <= 1) return;
    this.apiGetValues(this.page - 1);
  },

  btnNextClick: function () {
    if (this.page >= this.pageCount) return;
    this.apiGetValues(this.page + 1);
  },

  btnLastClick: function () {
    if (this.page >= this.pageCount) return;
    this.apiGetValues(this.pageCount);
  },

  btnPageClick: function (page) {
    this.apiGetValues(page);
  },

  btnPagerClick: function () {
    event.stopPropagation();
    $('#dropdown-pager').toggle();
  },

  displayType: function (inputType) {
    if (inputType === 'SelectOne') return '单选项';
    else if (inputType === 'SelectMultiple') return '多选项';
    else if (inputType === 'SelectCascading') return '级联选项';
    return '';
  },

  displayTags: function (field) {
    if (field.tags) {
      return field.tags.join(',');
    }
    return '';
  }
};

var $vue = new Vue({
  el: '#main',
  data: data,
  methods: methods,
  created: function() {
    this.getFields();
  }
});