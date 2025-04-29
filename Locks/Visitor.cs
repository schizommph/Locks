using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Locks
{
    interface Visitor<T>
    {
        public T VisitBinary(Binary binary);
        public T VisitGrouping(Grouping grouping);
        public T VisitLiteral(Literal literal);
        public T VisitUnary(Unary unary);
        public T VisitVariable(Variable variable);
        public T VisitAssign(Assign assign);
        public T VisitCall(Call call);
        public T VisitGet(Get get);
        public T VisitSet(Set set);
        public T VisitThis(This ths);
        public T VisitSuper(Super super);
        public void VisitBlockStmt(BlockStmt blockStmt);
        public void VisitFunctionStmt(FunctionStmt functionStmt);
        public void VisitClassStmt(ClassStmt classStmt);
        public void VisitReturnStmt(ReturnStmt returnStmt);
        public void VisitIfStmt(IfStmt ifStmt);
        public void VisitWhileStmt(WhileStmt whileStmt);
        public void VisitExpressionStmt(ExpressionStmt exprStmt);
        public void VisitPrintStmt(PrintStmt printStmt);
        public void VisitVarStmt(VarStmt varStmt);
    }
}
