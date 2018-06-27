<%@ Page Language="C#" Inherits="SS.Filter.Pages.PageSettings" %>
  <!DOCTYPE html>
  <html>

  <head>
    <meta charset="utf-8">
    <link href="assets/css/bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="assets/css/siteserver.min.css" rel="stylesheet" type="text/css" />
    <link href="assets/css/ionicons.min.css" rel="stylesheet" type="text/css" />
  </head>

  <body>
    <form id="form" runat="server">

      <!-- <ul class="nav nav-tabs tabs-bordered nav-justified">
        <li class="nav-item">
          <a class="nav-link" href="javascript:;">点击量统计</a>
        </li>
        <li class="nav-item">
          <a class="nav-link" href="javascript:;">下载量统计</a>
        </li>
        <li class="nav-item">
          <a class="nav-link active" href="javascript:;">选项设置</a>
        </li>
      </ul> -->

      <div class="m-3">
        <div class="card-box">
          <div class="row">
            <div class="col-lg-10">
              <h4 class="m-t-0 header-title">
                <b>选项设置</b>
              </h4>
              <p class="text-muted font-13 m-b-30">
                在此设置内容筛选选项
              </p>
            </div>
          </div>

          <asp:Literal id="LtlMessage" runat="server" />

          <div class="form-group row">
            <label class="col-3 col-form-label">是否启用点击量统计</label>
            <div class="col-3">
              <asp:DropDownList id="DdlIsHitsDisabled" class="form-control" runat="server">
                <asp:ListItem value="False">启用</asp:ListItem>
                <asp:ListItem value="True">禁用</asp:ListItem>
              </asp:DropDownList>
            </div>
            <div class="col-6 help-block">
              <small>禁用点击量统计，原有数据仍将保留</small>
            </div>
          </div>

          <div class="form-group row" style="display: none">
            <label class="col-3 col-form-label">是否启用下载量统计</label>
            <div class="col-3">
              <asp:DropDownList id="DdlIsDownloadsDisabled" class="form-control" runat="server">
                <asp:ListItem value="False">启用</asp:ListItem>
                <asp:ListItem value="True">禁用</asp:ListItem>
              </asp:DropDownList>
            </div>
            <div class="col-6 help-block">
              <small>禁用下载量统计，原有数据仍将保留</small>
            </div>
          </div>

          <hr />
          <asp:Button class="btn btn-primary" id="Submit" text="确 定" onclick="Submit_OnClick" runat="server" />
        </div>
      </div>



    </form>
  </body>

  </html>
  <script src="assets/js/jquery.min.js"></script>
  <script src="assets/layer/layer.min.js" type="text/javascript"></script>
  <script src="assets/sweetalert/sweetalert.min.js" type="text/javascript"></script>