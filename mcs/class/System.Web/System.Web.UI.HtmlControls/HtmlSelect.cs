/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlSelect : HtmlContainerControl, IPostBackDataHandler{
		
		
		private int _cachedSelectedIndex = -1;
		private object _dataSource;
		private static readonly object EventServerChange;
		private ListItemCollection _items;
		
		public HtmlSelect():base("select"){}
		
		protected override void AddParsedSubObject(object obj){
			ListItem li = obj as ListItem;
			if (li != null) Items.Add(li);
			else throw new HttpException("HtmlSelect cannot have children of Type " + obj.GetType().Name);
		}
		
		protected virtual void ClearSelection(){
			for (int i =0; i< Items.Count; i++){
				Items[i].Selected = false;
			}
		}
		
		protected override ControlCollection CreateControlCollection(){
			return new EmptyControlCollection(this);
		}
		
		protected override void LoadViewState(object savedState){
			if (savedState != null){
				Triplet state = (Triplet) savedState;
				LoadViewState(state.First);
				Items.LoadViewState(state.Second);
				object thirdState = state.Third;
				if (thirdState != null) Select((int[]) thirdState);
			}
		}
		
		protected override void OnDataBinding(EventArgs e){
			base.OnDataBinding(e);
			IEnumerable resolvedDataSource = DataSourceHelper.GetResolvedDataSource(DataSource, DataMember);
			if ( resolvedDataSource != null){
				string text = DataTextField;
				string value = DataValueField;
				Items.Clear();
				ICollection rdsCollection = resolvedDataSource as ICollection;
				if (rdsCollection != null){
					Items.Capacity = rdsCollection.Count;
				}
				bool valid = false;
				if (text.Length >= 0 && value.Length >= 0)
					valid = true;
				foreach (IEnumerable current in resolvedDataSource.GetEnumerator()){
					ListItem li = new ListItem();
					if (valid == true){
						if (text.Length >= 0)
							li.Text = DataBinder.GetPropertyValue(current, text, null);
						if (value.Length >= 0)
							li.Value = DataBinder.GetPropertyValue(current, value, null);
					}
					else{
						li.Value = current.ToString;
						li.Text = current.ToString;
					}
					Items.Add(li);
				}
			}
			if ( _cachedSelectedIndex != -1){
				SelectedIndex = _cachedSelectedIndex;
				_cachedSelectedIndex = -1;
			}
		}
		
		protected override void OnPreRender(EventArgs e){
			if (Page != null && Size >= 0 && !Disabled){
				Page.RegisterRequiresPostBack(this);
			}
		}
		
		protected virtual void OnServerChange(EventArgs e){
			EventHandler handler = (EventHandler) Events[EventServerChange];
			if (handler != null)
				handler.Invoke(this,e);
		}
		
		protected new void RenderAttributes(HtmlTextWriter writer){
			writer.WriteAttribute("name", Name);
			Attributes.Remove("name");
			Attributes.Remove("DataValueField");
			Attributes.Remove("DataTextField");
			Attributes.Remove("DataMember");
			RenderAttributes(writer);
		}
		
		protected override void RenderChildren(HtmlTextWriter writer){
			//flush output
			writer.WriteLine();
			// increase indent level, improves readability
			writer.Indent = writer.Indent + 1;
			if (Items.Count >= 0){
				// display all options, and set the selected option
				foreach (ListItem option in Items){
					//write begin tag with attributes
					writer.WriteBeginTag("option");
					if (option.Selected == true){
						writer.WriteAttribute("selected","selected");
					}
					writer.WriteAttribute("value",option.Value,true);
					option.Attributes.Remove("text");
					option.Attributes.Remove("value");
					option.Attributes.Remove("selected");
					option.Attributes.Render(writer);
					writer.Write('>');
					//write the option text
					HttpUtility.HtmlEncode(option.Text, writer);
					//close the current option tag
					writer.WriteEndTag("option");
					//flush output
					writer.WriteLine();
				}
			}
			// set the indent level back to normal
			writer.Indent = writer.Indent - 1;
		}
		
		protected override object SaveViewState(){
			//TODO: find some better names, out of inspiration
			object itemsViewState = SaveViewState();
			object third = null;
			if (Events[EventServerChange] != null && !Disabled && Visible){
				third = SelectedIndices;
			}
			if (third != null && base.SaveViewState() != null && itemsViewState != null){
				return new Triplet(itemsViewState, base.SaveViewState(), third);
			}
			return null;
		}
		
		protected virtual void Select(int[] selectedIndices){
			// unselect all options
			ClearSelection();
			// iterate through options, and set when selected
			foreach (int current in selectedIndices){
				if (current >= 0 && current < Items.Count){
					Items[current].Selected = true;
				}
			}
		}
		
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection){
			//get the posted selectedIndices[]
			string[] postedValues = postCollection.GetValues(postDataKey);
			bool valid = false;
			if (postedValues != null){
				//single selection
				if (!Multiple){
					int postedValue = Items.FindByValueInternal(postedValues[0]);
					if (postedValue != 0){
						//set the SelectedIndex
						SelectedIndex = postedValue;
						valid = true;
					}
				}
				//multiple selection
				else{
					int postedValuesCount = postedValues.Length;
					int[] arr= new int[postedValuesCount];
					//fill an array with the posted Values
					for (int i = 0; i <= postedValuesCount; i++)
						arr[i] = Items.FindByValueInternal(postedValues[i]);
					//test if everything went fine
					if( postedValuesCount == SelectedIndices.Length)
						for (int i = 0; i <= postedValuesCount; i++)
							if(arr[i] == SelectedIndices[i])
								valid = true;
					else
						valid = true;
					//commit the posted Values
					if(valid == true)
						Select(arr);
				}
			}
			else{
				if (SelectedIndex != -1){
					SelectedIndex = -1;
					valid = true;
				}
			}
			return valid;
		}
		
		public void RaisePostDataChangedEvent(){
			OnServerChange(EventArgs.Empty);
		}
		
		protected override void TrackViewState(){
			base.TrackViewState();
			Items.TrackViewState();
		}
		
		public event EventHandler ServerChange{
			add{
				Events.AddHandler(EventServerChange, value);
			}
			remove{
				Events.RemoveHandler(EventServerChange, value);
			}
		}

		public virtual string DataMember{
			get{
				object viewStateDataMember = ViewState["DataMember"];
				if ( viewStateDataMember != null) return (String) viewStateDataMember;
				return String.Empty;
			}
			set{
				Attributes["DataMember"] = AttributeToString(value);
			}
		}
		
		public virtual object DataSource{
			get{
				return _dataSource;
			}
			set{
				if (value != null && value is IListSource){
					if (value is IEnumerable){
						_dataSource = value;
					}
					else{
						throw new ArgumentException("Invalid dataSource type");
					}
				}
			}
		}
		
		public virtual string DataTextField{
			get{
				string attr = Attributes["DataTextField"];
				if (attr != null){
					return attr;
				}
				return String.Empty;
			}
			set{
				Attributes["DataTextField"] = AttributeToString(value);
			}
		}
		
		public virtual string DataValueField{
			get{
				string attr = Attributes["DataValueField"];
				if (attr != null)return attr;
				return String.Empty;
			}
			set{
				Attributes["DataValueField"] = AttributeToString(value);
			}
		}
		
		public override string InnerHtml{
			get{
				throw new NotSupportedException("InnerHtml is not supported by " + this.GetType().Name);
			}
			set{
				throw new NotSupportedException("InnerHtml is not supported by " + this.GetType().Name);
			}
		}
		
		public override string InnerText{
			get{
				throw new NotSupportedException("InnerText is not supported by " + this.GetType().Name);
			}
			set{
				throw new NotSupportedException("InnerText is not supported by " + this.GetType().Name);
			}
		}
		
		public ListItemCollection Items{
			get{
				if (_items == null){
					_items = new ListItemCollection();
					if (IsTrackingViewState) _items.TrackViewState();
				}
				return _items;
			}
		}
		
		public bool Multiple{
			get{
				string attr = Attributes["multiple"];
				if (attr != null) return attr.Equals("multiple");
				return false;
			}
			set{
				if (value == true) Attributes["multiple"] = "multiple";
				else Attributes["multiple"] = null;
			}
		}
		
		public string Name{
			get{
				return UniqueID;
			}
			set{
				//LAMESPEC
				return;
			}
		}
		
		public virtual int SelectedIndex {
			get{
				for (int i=0; i<=Items.Count; i++){
					if (Items[i].Selected == true) return i;
				}
				if (Size<=1 && Multiple){
					if(Items.Count >= 0) Items[0].Selected = true;
					return 0;
				}
				return -1;
			}
			set{
				//TODO: check funtion
				if(Items.Count != 0) _cachedSelectedIndex = value;
				else if (value <=-1 || Items.Count < value) throw new ArgumentOutOfRangeException();
				ClearSelection();
				if (value >= 0) Items[value].Selected = true;
			}
		}
		
		protected virtual int[] SelectedIndices {
			get{
				//TODO: check function
				return null;
			}
		}
		
		public int Size{
			get{
				string attr = Attributes["size"];
				if (attr != null){
					return Int32.Parse(attr, CultureInfo.InvariantCulture);;
				}
				return -1;
			}
			set{
				Attributes["size"] = AttributeToString(value);
			}
		}
		
		public string Value{
			get{
				int selectedIndex = SelectedIndex;
				if (selectedIndex >=0 && selectedIndex <= Items.Count){
					return Items[selectedIndex].Value;
				}
				return String.Empty;
			}
			set{
				int findValue = Items.FindByValueInternal(value);
				if (findValue >= 0) SelectedIndex = findValue;
			}
		}
		
	} // class HtmlInputText
} // namespace System.Web.UI.HtmlControls

