var $api = axios.create({
  baseURL: $apiUrl,
  params: {
    siteId: $siteId
  },
  withCredentials: true
});

var $data = {
  pageLoad: false,
  pageAlert: null,
  fields: null,
  results: null,
  top: 20,
  count: 0,
  pageCount: 0,
  page: 0
};

var $methods = {
  pageError: function (error) {
    pageUtils.loading(false);
    this.pageLoad = true;
    this.pageAlert = {
      type: 'danger',
      html: error.response.data.message
    };
  },
  getFields: function () {
    var $this = this;

    $api.get('fields')
      .then(function (response) {
        pageUtils.loading(false);
        $this.fields = response.data;
        $this.pageLoad = true;
      })
      .catch(function (error) {
        pageUtils.loading(false);
        $this.pageLoad = true;
        $this.pageAlert = {
          type: 'danger',
          html: error.response.data.message
        };
      });
  },
  getValues: function (page) {
    var $this = this;

    pageUtils.loading(true);

    $api.post('values/actions/search?top=' + this.top + '&skip=' + (this.top * (page - 1)), this.fields)
      .then(function (response) {
        pageUtils.loading(false);
        $this.results = response.data.value;
        $this.count = response.data.count;
        $this.page = page;
        $this.pageCount = Math.ceil($this.count / $this.top);

        //console.log('count:' + $this.count + ',page:' + $this.page + ',pageCount:' + $this.pageCount);
      })
      .catch($this.pageError);
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
    this.getValues(1);
  },
  btnFirstClick: function() {
    if (this.page <= 1) return;
    this.getValues(1);
  },
  btnPreviousClick: function() {
    if (this.page <= 1) return;
    this.getValues(this.page - 1);
  },
  btnNextClick: function() {
    if (this.page >= this.pageCount) return;
    this.getValues(this.page + 1);
  },
  btnLastClick: function() {
    if (this.page >= this.pageCount) return;
    this.getValues(this.pageCount);
  },
  btnPageClick: function(page) {
    this.getValues(page);
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
  data: $data,
  methods: $methods
});

$vue.getFields();