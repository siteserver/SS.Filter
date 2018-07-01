var $apiUrl = decodeURIComponent(new RegExp('[?&]apiUrl=([^&#]*)', 'i').exec(window.location.href)[1]) + '/plugins/ss.filter/';
var $siteId = parseInt(decodeURIComponent(new RegExp('[?&]siteId=([^&#]*)', 'i').exec(window.location.href)[1]));

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
  pageType: null,
  items: null,
  adminName: null,
  item: null,
  inputTypes: ['SelectOne', 'SelectMultiple'],
  tag: {}
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
        $this.items = response.data;
        $this.item = null;
        $this.pageLoad = true;
        $this.pageType = 'list';
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
  getTag: function (fieldId, tagId) {
    var $this = this;

    pageUtils.loading(true);
    $api.get('tags/' + tagId + '?fieldId=' + fieldId)
      .then(function (response) {
        pageUtils.loading(false);
        $this.tag = response.data;
        pageUtils.openLayer({
          domId: 'modal',
          title: '设置级联分类',
          width: 550,
          height: 350
        });
      })
      .catch($this.pageError);
  },
  updateTag: function () {
    var $this = this;
    pageUtils.closeLayer();
    pageUtils.loading(true);
    $api.put('tags/' + $this.tag.id, $this.tag)
      .then(function (response) {
        $this.pageAlert = {
          type: 'success',
          html: '筛选级联分类修改成功！'
        };
        $this.getFields();
      })
      .catch($this.pageError);
  },
  createField: function (item) {
    var $this = this;

    pageUtils.loading(true);
    $api.post('fields', item)
      .then(function (response) {
        $this.pageAlert = {
          type: 'success',
          html: '筛选分类添加成功！'
        };
        $this.getFields();
      })
      .catch($this.pageError);
  },
  updateField: function (item) {
    var $this = this;

    pageUtils.loading(true);
    $api.put('fields', item)
      .then(function (response) {
        $this.pageAlert = {
          type: 'success',
          html: '筛选分类修改成功！'
        };
        $this.getFields();
      })
      .catch($this.pageError);
  },
  deleteField: function (item) {
    var $this = this;

    pageUtils.loading(true);
    $api.delete('fields/' + item.id)
      .then(function (response) {
        $this.pageAlert = {
          type: 'success',
          html: '筛选分类删除成功！'
        };
        $this.getFields();
      })
      .catch($this.pageError);
  },
  btnAddClick: function () {
    this.pageType = 'add';
    this.pageAlert = null;
    this.item = {
      id: 0,
      siteId: $siteId,
      taxis: 0,
      title: '',
      inputType: 'SelectOne',
      tags: []
    };
  },
  btnEditClick: function (item) {
    this.pageType = 'add';
    this.pageAlert = null;
    this.item = item;
  },
  btnSubmitClick: function () {
    if (this.item.id) {
      this.updateField(this.item);
    } else {
      this.createField(this.item);
    }
  },
  btnCancelClick: function () {
    pageUtils.closeLayer();
    this.pageType = 'list';
    this.item = null;
  },
  btnTagClick: function (item, tagId) {
    this.getTag(item.id, tagId)
  },
  btnDeleteClick: function (item) {
    var $this = this;

    pageUtils.alertDelete({
      title: '删除筛选分类',
      text: '此操作将删除筛选分类 ' + item.title + '，确定吗？',
      callback: function () {
        $this.deleteField(item);
      }
    });
  },
  btnTagSubmitClick: function () {
    this.updateTag();
  },
  displayType: function (inputType) {
    if (inputType === 'SelectOne') return '单选项'
    else if (inputType === 'SelectMultiple') return '多选项'
    else if (inputType === 'SelectCascading') return '级联选项'
    return '';
  },
  displayTags: function (item) {
    if (item.tags) {
      return item.tags.join(',');
    }
    return '';
  }
};

Vue.component('input-tag', InputTag);

var $vue = new Vue({
  el: '#main',
  data: $data,
  methods: $methods
});

$vue.getFields();