using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppModel
{
    public class LangStringLib
    {
        public static List<LangValue> GetValues(NoodleDContext db, Guid rowGuid, int fieldTypeId)
        {
            List<LangValue> retVal = new List<LangValue>();
            try
            {
                foreach (StringValue item in
                    from s in db.StringValue where s.RowGUID == rowGuid && s.FieldType.Id == fieldTypeId select s)
                {
                    LangValue lv = new LangValue() { Lang = item.Lang, Value = item.Value };
                    retVal.Add(lv);
                }
            }
            catch (Exception)
            {
                retVal = null;
            }
            return retVal;
        }

        public static bool SetValues(NoodleDContext db, Guid rowGuid, int fieldTypeId, string strRu, string strUa, string strEn)
        {
            bool retVal = true;
            try
            {
                setTextToTable(db,rowGuid,fieldTypeId, "ru", strRu);
                setTextToTable(db,rowGuid,fieldTypeId, "ua", strUa);
                setTextToTable(db,rowGuid,fieldTypeId, "en", strEn);
                db.SaveChanges();
            }
            catch (Exception)
            {
                retVal = false;
            }
        return retVal;
        }
        private static void setTextToTable(NoodleDContext db, Guid rowGuid,int fieldTypeId, string lang, string value)
        {
            StringValue sVal = db.StringValue.FirstOrDefault(s => (s.RowGUID == rowGuid) && (s.FieldType.Id == fieldTypeId) && (s.Lang == lang));
            if (sVal == null)
            {
                StringValue newSVal = new StringValue() { RowGUID = rowGuid, Lang = lang, Value = value };
                newSVal.FieldType = db.FieldType.Find(fieldTypeId);
                db.StringValue.Add(newSVal);
            }
            else
            {
                sVal.Value = value;
            }

        }

        public static bool Delete(Guid rowGuid, int fieldTypeId)
        {
            bool retVal = true;
            try
            {
                using (NoodleDContext db = new NoodleDContext())
                {
                    db.StringValue.RemoveRange(from ls in db.StringValue where ls.RowGUID == rowGuid && (ls.FieldType.Id == fieldTypeId) select ls);
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                retVal = false;
            }
            return retVal;
        }

    } // class StringVals

    public class LangValue
    {
        public string Lang { get; set; }
        public string Value { get; set; }
    } 

    public class LangValues
    {
        public Guid RowGuid { get; set; }
        public int FieldTypeId { get; set; }
        public List<string> Values { get; set; }

        public LangValues()
        {
            Values = new List<string>();
        }
    }

}
