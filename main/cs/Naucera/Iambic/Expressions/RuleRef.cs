using System.Collections.Generic;
using System.Text;

namespace Naucera.Iambic.Expressions
{
	/// <summary>
	/// Expression which evaluates a named rule as a nested expression.
	/// </summary>
	/// 
	/// <remarks>
	/// <para>Copyright (C) 2010 by Amanda Koh.</para>
	/// </remarks>
	
	public class RuleRef : ParseExpression
	{
		private readonly string targetRuleName;
		private ParseRule targetRule;


		/// <summary>
		/// Creates a RuleRef which references a rule with the specified name.
		/// </summary>
		
		public RuleRef(string ruleName)
		{
			this.targetRuleName = ruleName;
		}


		/// <summary>
		/// Name of the referenced rule.
		/// </summary>
		
		public string TargetRuleName {
			get { return targetRuleName; }
		}


		internal override bool CheckWellFormed(string baseRuleName,
											   HashSet<string> ruleNames)
		{
			if (!ruleNames.Add(targetRuleName))
				throw new CircularDefinitionException(baseRuleName, targetRuleName);

			return targetRule.Expression.CheckWellFormed(targetRuleName, ruleNames);
		}


		internal override ParseExpression Compile<T>(Parser<T> parser)
		{
			targetRule = parser.GetRule(targetRuleName);
			if (targetRule == null)
				throw new UndefinedConstructException(targetRuleName);

			return this;
		}


		internal override bool Parse(ParseContext context,
									 ParseRule rule,
									 out Token result)
		{
			return this.targetRule.Parse(context, out result);
		}


		public override void ToString(StringBuilder text)
		{
			text.Append(targetRuleName);
		}
	}
}
