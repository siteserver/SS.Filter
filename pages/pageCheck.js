var $apiUrl = decodeURIComponent(new RegExp('[?&]apiUrl=([^&#]*)', 'i').exec(window.location.href)[1]) + '/plugins/ss.filter/';
var $siteId = parseInt(decodeURIComponent(new RegExp('[?&]siteId=([^&#]*)', 'i').exec(window.location.href)[1]));
var $channelId = parseInt(decodeURIComponent(new RegExp('[?&]channelId=([^&#]*)', 'i').exec(window.location.href)[1]));
var $contentId = parseInt(decodeURIComponent(new RegExp('[?&]contentId=([^&#]*)', 'i').exec(window.location.href)[1]));

var $api = axios.create({
  baseURL: $apiUrl,
  params: {
    siteId: $siteId,
    channelId: $channelId,
    contentId: $contentId
  },
  withCredentials: true
});

var $data = {
  pageLoad: false,
  pageAlert: null,
  items: null
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
  getValues: function () {
    var $this = this;

    $api.get('fields')
      .then(function (response) {
        pageUtils.loading(false);
        $this.items = response.data;
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
  updateValue: function (item, tagInfo) {
    var $this = this;

    pageUtils.loading(true);
    $api.put('values/' + item.id, {
      isMultiple: item.inputType === 'SelectMultiple',
      isAdd: item.checkedTagIds.indexOf(tagInfo.id) === -1,
      tagId: tagInfo.id
    })
      .then(function (response) {
        $this.getValues();
      })
      .catch($this.pageError);
  },
  btnTagClick: function (item, tagInfo) {
    this.updateValue(item, tagInfo);
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

var $vue = new Vue({
  el: '#main',
  data: $data,
  methods: $methods
});

$vue.getValues();